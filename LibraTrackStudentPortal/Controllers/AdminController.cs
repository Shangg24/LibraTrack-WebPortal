using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using LibraTrackStudentPortal.Data;

namespace LibraTrackStudentPortal.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Role") != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalLibrarians = _context.users.Count(u => u.role == "Librarian");
            ViewBag.ActiveLibrarians = _context.users.Count(u => u.role == "Librarian" && u.status == "Active");
            ViewBag.TotalStudents = _context.Students.Count();
            ViewBag.ActiveStudents = _context.Students.Count(s => s.IsActive);

            return View();
        }
    }
}