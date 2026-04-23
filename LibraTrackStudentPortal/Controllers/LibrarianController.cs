using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using LibraTrackStudentPortal.Data;

namespace LibraTrackStudentPortal.Controllers
{
    public class LibrarianController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LibrarianController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalBooks = _context.books.Count();
            ViewBag.AvailableBooks = _context.books.Count(b => b.available > 0);
            ViewBag.PendingRequests = _context.book_requests.Count(r => r.status == "Pending");
            ViewBag.BorrowedBooks = _context.issue_books.Count(i => i.status == "Borrowed");

            return View();
        }
    }
}