using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using LibraTrackStudentPortal.Data;
using LibraTrackStudentPortal.Models;

namespace LibraTrackStudentPortal.Controllers
{
    public class IssueBooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IssueBooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var reservedRequests = (from r in _context.book_requests
                                    join b in _context.books on r.book_id equals b.id
                                    join s in _context.Students on r.ID_no equals s.ID_no
                                    where r.status == "Reserved"
                                    select new
                                    {
                                        ID_no = r.ID_no ?? "",
                                        StudentName = s.full_name ?? "",
                                        StudentEmail = s.email ?? "",
                                        GradeSection = s.grade_section ?? "",
                                        RequestNo = r.request_no ?? "",
                                        BookId = b.id ?? "",
                                        BookTitle = b.book_title ?? "",
                                        Author = b.author ?? ""
                                    }).ToList();

            var grouped = reservedRequests
                .GroupBy(r => new { r.ID_no, r.StudentName, r.StudentEmail, r.GradeSection })
                .Select(g => new IssueTransactionViewModel
                {
                    StudentID = g.Key.ID_no,
                    StudentName = g.Key.StudentName,
                    StudentEmail = g.Key.StudentEmail,
                    GradeSection = g.Key.GradeSection,
                    ReservedCount = g.Count(),
                    RequestNos = g.Select(x => x.RequestNo).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList(),
                    Books = g.Select(x => new IssueBookDetailViewModel
                    {
                        BookId = x.BookId,
                        BookTitle = x.BookTitle,
                        Author = x.Author
                    }).ToList()
                })
                .ToList();

            ViewBag.TotalTransactionsReady = grouped.Count;
            ViewBag.TotalReservedBooks = reservedRequests.Count;

            int pageSize = 20;
            int totalTransactions = grouped.Count;

            var pagedGrouped = grouped
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalTransactions / pageSize);

            var lastIssueId = _context.issues
                .Select(i => i.issue_id)
                .Where(x => x != null && x.StartsWith("I-"))
                .OrderByDescending(x => x)
                .FirstOrDefault();

            string nextIssueId = "I-0001";

            if (!string.IsNullOrWhiteSpace(lastIssueId))
            {
                var numericPart = lastIssueId.Replace("I-", "");
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextIssueId = "I-" + (lastNumber + 1).ToString("D4");
                }
            }

            ViewBag.NextIssueId = nextIssueId;

            return View(pagedGrouped);
        }

        public IActionResult ProcessIssue(string id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students.FirstOrDefault(s => s.ID_no == id);

            if (student == null)
            {
                TempData["Success"] = "Student not found.";
                return RedirectToAction("Index");
            }

            var reservedRequests = _context.book_requests
                .Where(r => r.ID_no == id && r.status == "Reserved")
                .ToList();

            if (!reservedRequests.Any())
            {
                TempData["Success"] = "No reserved books found for this student.";
                return RedirectToAction("Index");
            }

            var reservedBookIds = reservedRequests.Select(r => r.book_id).ToList();

            var books = _context.books
                .Where(b => reservedBookIds.Contains(b.id))
                .ToList();

            if (books.Count != reservedBookIds.Count)
            {
                TempData["Success"] = "One or more books could not be found.";
                return RedirectToAction("Index");
            }

            if (books.Any(b => b.available <= 0))
            {
                TempData["Success"] = "One or more reserved books have no available copies.";
                return RedirectToAction("Index");
            }


            // Generate issue_id like I-0001
            var lastIssueId = _context.issues
                .Select(i => i.issue_id)
                .Where(x => x != null && x.StartsWith("I-"))
                .OrderByDescending(x => x)
                .FirstOrDefault();

            string newIssueId = "I-0001";

            if (!string.IsNullOrWhiteSpace(lastIssueId))
            {
                var numericPart = lastIssueId.Replace("I-", "");
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    newIssueId = "I-" + (lastNumber + 1).ToString("D4");
                }
            }

            int lastIssueTableId = _context.issues.Any()
            ? _context.issues.Max(i => i.id)
            : 0;

            lastIssueTableId++;

            // Create one issue transaction
            var newIssue = new issue
            {
                id = lastIssueTableId,
                issue_id = newIssueId,
                full_name = student.full_name,
                contact = student.contact,
                email = student.email,
                ID_no = student.ID_no,
                grade_section = student.grade_section,
                issue_date = DateTime.Now,
                return_date = DateTime.Now.AddDays(7),
                date_insert = DateTime.Now,
                status = "Borrowed"
            };

            _context.issues.Add(newIssue);

            int lastIssueBookId = _context.issue_books.Any()
                ? _context.issue_books.Max(x => x.id)
                : 0;

            // Create multiple issue_books rows under one issue_id
            foreach (var request in reservedRequests)
            {
                lastIssueBookId++;

                var newIssueBook = new issue_books
                {
                    id = lastIssueBookId,
                    issue_id = newIssueId,
                    book_id = request.book_id,
                    status = "Borrowed",
                    date_insert = DateTime.Now
                };

                _context.issue_books.Add(newIssueBook);

                request.status = "Borrowed";

                var book = books.First(b => b.id == request.book_id);
                book.available -= 1;
            }

            _context.SaveChanges();

            TempData["Success"] = $"Books issued successfully under transaction {newIssueId}.";
            return RedirectToAction("Index");
        }
    }
}