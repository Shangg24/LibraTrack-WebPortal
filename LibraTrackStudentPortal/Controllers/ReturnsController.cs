using LibraTrackStudentPortal.Data;
using LibraTrackStudentPortal.Models;
using LibraTrackStudentPortal.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LibraTrackStudentPortal.Controllers
{
    public class ReturnsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public ReturnsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

        public async Task<IActionResult> SendOverdueNotices()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var overdueRows = (from i in _context.issues
                               join ib in _context.issue_books on i.issue_id equals ib.issue_id
                               join b in _context.books on ib.book_id equals b.id
                               join s in _context.Students on i.ID_no equals s.ID_no
                               where ib.status == "Borrowed"
                                     && i.return_date.Date < DateTime.Today
                                     && s.email != null
                               select new
                               {
                                   StudentEmail = s.email,
                                   StudentName = s.full_name,
                                   BookTitle = b.book_title,
                                   DueDate = i.return_date,
                                   DaysOverdue = (DateTime.Today - i.return_date.Date).Days
                               }).ToList();

            if (!overdueRows.Any())
            {
                TempData["Success"] = "No overdue borrowed books found.";
                return RedirectToAction("Index");
            }

            var groupedByStudent = overdueRows.GroupBy(x => new
            {
                x.StudentEmail,
                x.StudentName
            });

            int sentCount = 0;

            foreach (var studentGroup in groupedByStudent)
            {
                var bookRows = new StringBuilder();

                foreach (var item in studentGroup)
                {
                    bookRows.AppendLine($@"
                <tr>
                    <td style='border: 1px solid #ccc; padding: 8px;'>{item.BookTitle}</td>
                    <td style='border: 1px solid #ccc; padding: 8px;'>{item.DueDate:MMMM dd, yyyy}</td>
                    <td style='border: 1px solid #ccc; padding: 8px; text-align: center;'>{item.DaysOverdue} day(s)</td>
                </tr>");
                }

                var body = $@"
            <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                <p>Hi {studentGroup.Key.StudentName},</p>

                <p>Just a quick reminder that you still have overdue book(s) from the library:</p>

                <table style='border-collapse: collapse; width: 100%; margin-top: 10px;'>
                    <thead>
                        <tr style='background-color: #f2f2f2;'>
                            <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Book Title</th>
                            <th style='border: 1px solid #ccc; padding: 8px; text-align: left;'>Due Date</th>
                            <th style='border: 1px solid #ccc; padding: 8px; text-align: center;'>Days Overdue</th>
                        </tr>
                    </thead>
                    <tbody>
                        {bookRows}
                    </tbody>
                </table>

                <p style='margin-top: 15px;'>
                    If you’ve already returned these, you may ignore this message.
                    Otherwise, please return them as soon as you can.
                </p>

                <p>
                    Thanks,<br>
                    LibraTrack Team
                </p>
            </div>";

                await _emailService.SendEmailAsync(
                    studentGroup.Key.StudentEmail,
                    "Overdue Book Reminder - LibraTrack",
                    body
                );

                sentCount++;
            }

            TempData["Success"] = $"Overdue email notices sent successfully to {sentCount} student(s).";

            return RedirectToAction("Index");
        }

    }
}