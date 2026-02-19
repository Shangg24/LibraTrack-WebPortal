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
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Student model)
        {
            var student = _context.Students
                .FirstOrDefault(s => s.ID_no == model.ID_no);

            if (student == null)
            {
                ViewBag.Error = "ID not found";
                return View();
            }

            if (student.password != model.password)
            {
                ViewBag.Error = "Wrong password";
                return View();
            }

            HttpContext.Session.SetString("StudentID", student.ID_no);

            return RedirectToAction("Dashboard", "Student");
        }




        [HttpPost]
        public IActionResult Register(Student student)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.Students
                    .FirstOrDefault(s => s.ID_no == student.ID_no);

                if (existing != null)
                {
                    ViewBag.Error = "Student ID already registered.";
                    return View();
                }

                _context.Students.Add(student);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(student);
        }

    }
}
