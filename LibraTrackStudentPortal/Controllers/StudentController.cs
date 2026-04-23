using System;
using System.Linq;
using LibraTrackStudentPortal.Data;
using LibraTrackStudentPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard(int currentPage = 1, int historyPage = 1)
    {
        if (HttpContext.Session.GetString("Role") != "Student")
        {
            return RedirectToAction("Login", "Account");
        }

        if (HttpContext.Session.GetString("MustChangePassword") == "true")
        {
            return RedirectToAction("ForceChangePassword");
        }

        var ID_no = HttpContext.Session.GetString("StudentID");

        if (string.IsNullOrEmpty(ID_no))
        {
            return RedirectToAction("Login", "Account");
        }

        var allTransactions = (from i in _context.issues
                               join ib in _context.issue_books on i.issue_id equals ib.issue_id
                               join b in _context.books on ib.book_id equals b.id
                               where i.ID_no.Trim() == ID_no.Trim()
                               select new BorrowedBooksViewModel
                               {
                                   BookTitle = b.book_title,
                                   IssueDate = i.issue_date,
                                   ReturnDate = i.return_date,
                                   Status = ib.status
                               }).ToList();

        var currentlyBorrowed = allTransactions
            .Where(x => x.Status == "Borrowed")
            .OrderByDescending(x => x.IssueDate)
            .ToList();

        var returnedBooks = allTransactions
            .Where(x => x.Status == "Returned" || x.Status == "Completed")
            .OrderByDescending(x => x.ReturnDate)
            .ToList();

        ViewBag.TotalTransactions = allTransactions.Count;
        ViewBag.CurrentBorrowed = currentlyBorrowed.Count;
        ViewBag.TotalReturned = returnedBooks.Count;

        int pageSize = 10;

        // Currently Borrowed pagination
        var pagedCurrentlyBorrowed = currentlyBorrowed
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentlyBorrowed = pagedCurrentlyBorrowed;
        ViewBag.CurrentPage = currentPage;
        ViewBag.TotalCurrentPages = (int)Math.Ceiling((double)currentlyBorrowed.Count / pageSize);

        // Borrowing History pagination
        var pagedReturnedBooks = returnedBooks
            .Skip((historyPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.ReturnedBooks = pagedReturnedBooks;
        ViewBag.HistoryPage = historyPage;
        ViewBag.TotalHistoryPages = (int)Math.Ceiling((double)returnedBooks.Count / pageSize);

        return View();
    }

    public IActionResult RequestBook()
    {
       
        if (HttpContext.Session.GetString("Role") != "Student")
        {
            return RedirectToAction("Login", "Account");
        }

        if (HttpContext.Session.GetString("MustChangePassword") == "true")
        {
            return RedirectToAction("ForceChangePassword");
        }

        var studentId = HttpContext.Session.GetString("StudentID");

        var books = _context.books.ToList();

        // existing requested books
        var requestedBookIds = _context.book_requests
        .Where(r => r.ID_no == studentId && (r.status == "Pending" || r.status == "Reserved"))
        .Select(r => r.book_id)
        .ToList();

        // 🔴 NEW: borrowed books
        var borrowedBookIds = (from i in _context.issues
                               join ib in _context.issue_books on i.issue_id equals ib.issue_id
                               where i.ID_no == studentId && ib.status == "Borrowed"
                               select ib.book_id).ToList();

        ViewBag.RequestedBooks = requestedBookIds;
        ViewBag.BorrowedBooks = borrowedBookIds; // 👈 ADD THIS

        return View(books);
    }

    [HttpPost]
    public IActionResult SubmitRequest(List<string> bookIds)
    {
        if (HttpContext.Session.GetString("Role") != "Student")
        {
            return RedirectToAction("Login", "Account");
        }

        if (HttpContext.Session.GetString("MustChangePassword") == "true")
        {
            return RedirectToAction("ForceChangePassword");
        }

        var studentId = HttpContext.Session.GetString("StudentID");

        if (string.IsNullOrEmpty(studentId))
        {
            return RedirectToAction("Login", "Account");
        }

        if (bookIds == null || !bookIds.Any())
        {
            TempData["ErrorMessage"] = "Please select at least one book.";
            return RedirectToAction("RequestBook");
        }

        // Generate ONE request_no for this whole transaction
        var lastRequestNo = _context.book_requests
            .Select(r => r.request_no)
            .Where(r => r != null && r.StartsWith("REQ-"))
            .OrderByDescending(r => r)
            .FirstOrDefault();

        string newRequestNo = "REQ-0001";

        if (!string.IsNullOrWhiteSpace(lastRequestNo))
        {
            var numericPart = lastRequestNo.Replace("REQ-", "");
            if (int.TryParse(numericPart, out int lastNumber))
            {
                newRequestNo = "REQ-" + (lastNumber + 1).ToString("D4");
            }
        }

        int lastRequestId = _context.book_requests.Any()
            ? _context.book_requests.Max(r => r.request_id)
            : 0;

        foreach (var bookId in bookIds)
        {
            var book = _context.books.FirstOrDefault(b => b.id == bookId);

            if (book == null)
            {
                continue;
            }

            if (book.available <= 0)
            {
                continue;
            }

            var alreadyBorrowed = (from i in _context.issues
                                   join ib in _context.issue_books on i.issue_id equals ib.issue_id
                                   where i.ID_no == studentId
                                         && ib.book_id == bookId
                                         && ib.status == "Borrowed"
                                   select ib).FirstOrDefault();

            if (alreadyBorrowed != null)
            {
                continue;
            }

            var existingRequest = _context.book_requests
                .FirstOrDefault(r => r.book_id == bookId
                                  && r.ID_no == studentId
                                  && (r.status == "Pending" || r.status == "Reserved"));

            if (existingRequest != null)
            {
                continue;
            }

            lastRequestId++;

            var request = new book_requests
            {
                request_id = lastRequestId,
                ID_no = studentId,
                book_id = bookId,
                request_no = newRequestNo,
                request_date = DateTime.Now,
                status = "Pending"
            };

            _context.book_requests.Add(request);
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = $"Request submitted successfully. Request No: {newRequestNo}";
        return RedirectToAction("RequestBook");
    }

    public IActionResult ForceChangePassword()
    {
        var studentId = HttpContext.Session.GetString("StudentID");

        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Login", "Account");

        return View();
        
    }

    [HttpPost]
    public IActionResult ForceChangePassword(string newPassword, string confirmPassword)
    {
        var studentId = HttpContext.Session.GetString("StudentID");

        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Login", "Account");

        if (newPassword != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            return View();
        }

        var student = _context.Students.FirstOrDefault(s => s.ID_no == studentId);

        if (student == null)
            return RedirectToAction("Login", "Account");

        // 🔐 UPDATE PASSWORD
        student.passwordHash = newPassword; // (or hashed version if you use hashing)

        // 🔓 RELEASE FIRST LOGIN RESTRICTION
        student.IsFirstLogin = false;
        student.MustChangePassword = false;

        _context.SaveChanges();

        // 🧠 UPDATE SESSION FLAG
        HttpContext.Session.Remove("MustChangePassword");

        return RedirectToAction("Dashboard", "Student");
    }

    public IActionResult ChangePassword()
    {
        var studentId = HttpContext.Session.GetString("StudentID");

        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Login", "Account");

        return View();
    }

    [HttpPost]
    public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var studentId = HttpContext.Session.GetString("StudentID");

        var student = _context.Students
            .FirstOrDefault(s => s.ID_no == studentId);

        if (student == null)
            return RedirectToAction("Login", "Account");

        if (student.passwordHash != currentPassword)
        {
            ViewBag.Error = "Current password is incorrect.";
            return View();
        }

        if (newPassword != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            return View();
        }

        student.passwordHash = newPassword;
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Password updated successfully.";

        return RedirectToAction("Dashboard");
    }


    public JsonResult SearchBooks(string term)
    {
        var books = _context.books
            .Where(b => b.book_title.Contains(term) || b.author.Contains(term))
            .Select(b => new
            {
                b.book_title,
                b.author
            })
            .Take(5)
            .ToList();

        return Json(books);
    }

}
