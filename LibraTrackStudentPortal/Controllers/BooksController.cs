using Microsoft.AspNetCore.Mvc;
using LibraTrackStudentPortal.Data;
using System.Linq;

namespace LibraTrackStudentPortal.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var books = _context.books.ToList();
            return View(books);
        }
    }
}
