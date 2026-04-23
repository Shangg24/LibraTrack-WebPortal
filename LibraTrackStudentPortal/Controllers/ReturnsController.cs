using LibraTrackStudentPortal.Data;
using LibraTrackStudentPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraTrackStudentPortal.Controllers
{
    public class ReturnsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReturnsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1, int historyPage = 1)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            // ACTIVE BORROWED TRANSACTIONS
            var borrowedRows = (from i in _context.issues
                                join ib in _context.issue_books on i.issue_id equals ib.issue_id
                                join b in _context.books on ib.book_id equals b.id
                                join s in _context.Students on i.ID_no equals s.ID_no into studentJoin
                                from s in studentJoin.DefaultIfEmpty()
                                where ib.status == "Borrowed"
                                select new
                                {
                                    IssueId = i.issue_id ?? "",
                                    StudentID = i.ID_no ?? "",
                                    StudentName = s != null ? (s.full_name ?? "") : "",
                                    IssueDate = i.issue_date,
                                    ReturnDate = i.return_date,
                                    Status = ib.status ?? "",
                                    BookId = b.id ?? "",
                                    BookTitle = b.book_title ?? "",
                                    Author = b.author ?? ""
                                }).ToList();

            var groupedReturns = borrowedRows
                .GroupBy(x => new { x.IssueId, x.StudentID, x.StudentName, x.IssueDate, x.ReturnDate, x.Status })
                .Select(g => new ReturnTransactionViewModel
                {
                    IssueId = g.Key.IssueId,
                    StudentID = g.Key.StudentID,
                    StudentName = g.Key.StudentName,
                    IssueDate = g.Key.IssueDate,
                    ReturnDate = g.Key.ReturnDate,
                    Status = g.Key.Status,
                    BookCount = g.Count(),

                    ReturnIndicator =
                        DateTime.Today > g.Key.ReturnDate.Date ? "Overdue" :
                        (g.Key.ReturnDate.Date - DateTime.Today).Days <= 1 ? "Due Soon" :
                        "On Time",

                    DaysOverdue =
                        DateTime.Today > g.Key.ReturnDate.Date
                            ? (DateTime.Today - g.Key.ReturnDate.Date).Days
                            : 0,

                    Books = g.Select(x => new ReturnBookDetail
                    {
                        BookId = x.BookId,
                        BookTitle = x.BookTitle,
                        Author = x.Author
                    }).ToList()
                })
                .OrderByDescending(x => x.IssueDate)
                .ToList();

            // COMPLETED / RETURN HISTORY
            var completedRows = (from i in _context.issues
                                 join ib in _context.issue_books on i.issue_id equals ib.issue_id
                                 join b in _context.books on ib.book_id equals b.id
                                 join s in _context.Students on i.ID_no equals s.ID_no into studentJoin
                                 from s in studentJoin.DefaultIfEmpty()
                                 where i.status == "Completed" || ib.status == "Completed" || ib.status == "Returned"
                                 select new
                                 {
                                     IssueId = i.issue_id ?? "",
                                     StudentID = i.ID_no ?? "",
                                     StudentName = s != null ? (s.full_name ?? "") : "",
                                     IssueDate = i.issue_date,
                                     ReturnDate = i.return_date,
                                     Status = i.status ?? ib.status ?? "",
                                     BookId = b.id ?? "",
                                     BookTitle = b.book_title ?? "",
                                     Author = b.author ?? ""
                                 }).ToList();

            var groupedCompleted = completedRows
                .GroupBy(x => new { x.IssueId, x.StudentID, x.StudentName, x.IssueDate, x.ReturnDate, x.Status })
                .Select(g => new ReturnTransactionViewModel
                {
                    IssueId = g.Key.IssueId,
                    StudentID = g.Key.StudentID,
                    StudentName = g.Key.StudentName,
                    IssueDate = g.Key.IssueDate,
                    ReturnDate = g.Key.ReturnDate,
                    Status = g.Key.Status,
                    BookCount = g.Count(),
                    ReturnIndicator = "Returned",
                    DaysOverdue = 0,
                    Books = g.Select(x => new ReturnBookDetail
                    {
                        BookId = x.BookId,
                        BookTitle = x.BookTitle,
                        Author = x.Author
                    }).ToList()
                })
                .OrderByDescending(x => x.ReturnDate)
                .ToList();

            int historyPageSize = 20;
            int totalHistory = groupedCompleted.Count;

            var pagedCompleted = groupedCompleted
                .Skip((historyPage - 1) * historyPageSize)
                .Take(historyPageSize)
                .ToList();

            ViewBag.HistoryPage = historyPage;
            ViewBag.TotalHistoryPages = (int)Math.Ceiling((double)totalHistory / historyPageSize);

            ViewBag.TotalBorrowedTransactions = groupedReturns.Count;
            ViewBag.TotalBorrowedBooks = borrowedRows.Count;
            ViewBag.TotalCompletedTransactions = groupedCompleted.Count;

            // pagination for active borrowed only
            int pageSize = 20;
            int totalReturns = groupedReturns.Count;

            var pagedReturns = groupedReturns
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalReturns / pageSize);

            // history shown separately
            ViewBag.CompletedTransactions = pagedCompleted;

            return View(pagedReturns);
        }

        public IActionResult ProcessReturn(string issueId)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(issueId))
            {
                return RedirectToAction("Index");
            }

            // Get only the safe fields we need
            var issueInfo = _context.issues
                .Where(i => i.issue_id == issueId)
                .Select(i => new
                {
                    i.issue_id,
                    i.ID_no
                })
                .FirstOrDefault();

            if (issueInfo == null)
            {
                return NotFound();
            }

            var issueBooks = _context.issue_books
                .Where(ib => ib.issue_id == issueId && ib.status == "Borrowed")
                .ToList();

            if (!issueBooks.Any())
            {
                TempData["Success"] = "No borrowed books found for this transaction.";
                return RedirectToAction("Index");
            }

            // Update issue_books and restore book availability
            foreach (var issueBook in issueBooks)
            {
                issueBook.status = "Completed";

                var book = _context.books.FirstOrDefault(b => b.id == issueBook.book_id);
                if (book != null)
                {
                    book.available += 1;
                }
            }

            // Update related request rows
            var issuedBookIds = issueBooks.Select(x => x.book_id).ToList();

            var relatedRequests = _context.book_requests
                .Where(r => r.ID_no == issueInfo.ID_no
                         && issuedBookIds.Contains(r.book_id)
                         && r.status == "Borrowed")
                .ToList();

            foreach (var request in relatedRequests)
            {
                request.status = "Completed";
            }

            _context.SaveChanges();

            // Update issues table directly
            _context.Database.ExecuteSqlRaw(
                "UPDATE issues SET status = {0}, return_date = {1} WHERE issue_id = {2}",
                "Completed",
                DateTime.Now,
                issueId
            );

            TempData["Success"] = $"Return processed successfully for transaction {issueId}.";
            return RedirectToAction("Index");
        }
    }
}