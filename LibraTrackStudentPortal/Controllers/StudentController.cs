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

        var request = new book_requests
        {
            ID_no = studentId,
            book_id = bookId,
            request_date = DateTime.Now,
            status = "Pending"
        };

        _context.book_requests.Add(request);
        _context.SaveChanges();

        return RedirectToAction("Dashboard");
    }

}
