using Microsoft.AspNetCore.Mvc;
using LibraTrackStudentPortal.Data;
using System.Linq;
using LibraTrackStudentPortal.Models;

namespace LibraTrackStudentPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public IActionResult Login(Student model)
        {
            var student = _context.Students
                .FirstOrDefault(s => s.ID_no == model.ID_no);

            if (student == null)
            {
                ViewBag.Error = "ID not found.";
                return View();
            }

            if (!student.IsActive)
            {
                ViewBag.Error = "Your account has been deactivated. Please contact the librarian.";
                return View();
            }

            if (student.passwordHash != model.passwordHash)
            {
                ViewBag.Error = "Wrong password.";
                return View();
            }

            if (student.IsFirstLogin)
            {
                HttpContext.Session.SetString("StudentID", student.ID_no);
                HttpContext.Session.SetString("StudentName", student.full_name);
                return RedirectToAction("ForceChangePassword", "Student");
            }

            HttpContext.Session.SetString("StudentID", student.ID_no);
            HttpContext.Session.SetString("StudentName", student.full_name);

            return RedirectToAction("Dashboard", "Student");
        }



    }
}
