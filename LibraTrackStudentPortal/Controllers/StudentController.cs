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

    public IActionResult Dashboard()
    {
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
            .ToList();

        var returnedBooks = allTransactions
            .Where(x => x.Status == "Returned")
            .ToList();

        ViewBag.CurrentlyBorrowed = currentlyBorrowed;
        ViewBag.ReturnedBooks = returnedBooks;

        ViewBag.TotalTransactions = allTransactions.Count;
        ViewBag.CurrentBorrowed = currentlyBorrowed.Count;
        ViewBag.TotalReturned = returnedBooks.Count;

        return View();
    }

    public IActionResult RequestBook()
    {
        var books = _context.books.ToList();
        return View(books);
    }

    [HttpPost]
    public IActionResult SubmitRequest(string bookId)
    {
        var studentId = HttpContext.Session.GetString("StudentID");

        if (string.IsNullOrEmpty(studentId))
        {
            return RedirectToAction("Login", "Account");
        }

        // ✅ ADD THIS PART HERE
        var existingRequest = _context.book_requests
            .FirstOrDefault(r => r.book_id == bookId
                              && r.ID_no == studentId
                              && r.status == "Pending");

        if (existingRequest != null)
        {
            TempData["SuccessMessage"] = "You already have a pending request for this book.";
            return RedirectToAction("Dashboard");
        }

        // ✅ ONLY CREATE REQUEST IF NO DUPLICATE
        var request = new book_requests
        {
            ID_no = studentId,
            book_id = bookId,
            request_date = DateTime.Now,
            status = "Pending"
        };

        _context.book_requests.Add(request);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Your reservation request has been submitted successfully.";

        return RedirectToAction("Dashboard");
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

        var student = _context.Students
            .FirstOrDefault(s => s.ID_no == studentId);

        if (student == null)
            return RedirectToAction("Login", "Account");

        student.passwordHash = newPassword; // we will hash later
        student.IsFirstLogin = false;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Password changed successfully.";

        return RedirectToAction("Dashboard");
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
