using System;
using LibraTrackStudentPortal.Data;
using LibraTrackStudentPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace LibraTrackStudentPortal.Controllers
{
    public class StudentAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            if (HttpContext.Session.GetString("Role") != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            int pageSize = 20;

            var query = _context.Students.AsQueryable();

            ViewBag.TotalStudents = query.Count();
            ViewBag.ActiveStudents = query.Count(s => s.IsActive);
            ViewBag.DeactivatedStudents = query.Count(s => !s.IsActive);

            var students = query
                .OrderByDescending(s => s.date_registered)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalStudents / pageSize);

            return View(students);
        }

        public IActionResult Activate(string id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students.FirstOrDefault(s => s.ID_no == id);

            if (student == null)
            {
                return NotFound();
            }

            student.IsActive = true;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Deactivate(string id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students.FirstOrDefault(s => s.ID_no == id);

            if (student == null)
            {
                return NotFound();
            }

            student.IsActive = false;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.GradeSections = _context.GradeSections
            .Where(g => g.is_active)
            .Select(g => new
            {
                Value = g.grade_level + "_" + g.section,
                Text = g.grade_level + " - " + g.section
            })
            .Distinct()
            .OrderBy(g => g.Text)
            .Select(g => new SelectListItem
            {
                Value = g.Value,
                Text = g.Text
            })
            .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(Student student, string plainPassword)
        {
            if (HttpContext.Session.GetString("Role") != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(student.ID_no) ||
                string.IsNullOrWhiteSpace(student.full_name) ||
                string.IsNullOrWhiteSpace(student.email) ||
                string.IsNullOrWhiteSpace(plainPassword))
            {
                ViewBag.Error = "Please complete all required fields.";
                ViewBag.GradeSections = _context.GradeSections
                    .Where(g => g.is_active)
                    .OrderBy(g => g.grade_level)
                    .ThenBy(g => g.section)
                    .Select(g => new SelectListItem
                    {
                        Value = g.grade_level + "_" + g.section,
                        Text = g.grade_level + " - " + g.section
                    })
                    .ToList();
                return View(student);
            }

            var existing = _context.Students.FirstOrDefault(s => s.ID_no == student.ID_no);
            if (existing != null)
            {
                ViewBag.Error = "Student ID already exists.";
                ViewBag.GradeSections = _context.GradeSections
                    .Where(g => g.is_active)
                    .OrderBy(g => g.grade_level)
                    .ThenBy(g => g.section)
                    .Select(g => new SelectListItem
                    {
                        Value = g.grade_level + "_" + g.section,
                        Text = g.grade_level + " - " + g.section
                    })
                    .ToList();
                return View(student);
            }

            student.passwordHash = plainPassword;
            student.IsActive = true;
            student.IsFirstLogin = true;
            student.MustChangePassword = true;


            student.date_registered = DateTime.Now;
            _context.Students.Add(student);
            _context.SaveChanges();

            TempData["Success"] = "Student account created successfully.";
            return RedirectToAction("Index");
        }

        public IActionResult ResetPassword(string id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students.FirstOrDefault(s => s.ID_no == id);

            if (student == null)
            {
                return NotFound();
            }

            string newPassword = "123456"; // temporary password

            student.passwordHash = newPassword;
            student.IsFirstLogin = true;
            student.MustChangePassword = true;

            _context.SaveChanges();

            TempData["Success"] = $"Password reset successful. Temporary password: {newPassword}";

            return RedirectToAction("Index");
        }

        public IActionResult Edit(string id)
        {
            if (HttpContext.Session.GetString("Role") != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students.FirstOrDefault(s => s.ID_no == id);
            if (student == null)
            {
                return NotFound();
            }

            ViewBag.GradeSections = _context.GradeSections
            .Where(g => g.is_active)
            .Select(g => new
            {
                Value = g.grade_level + "_" + g.section,
                Text = g.grade_level + " - " + g.section
            })
            .Distinct()
            .OrderBy(g => g.Text)
            .Select(g => new SelectListItem
            {
                Value = g.Value,
                Text = g.Text
            })
            .ToList();

            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(Student updatedStudent)
        {
            if (HttpContext.Session.GetString("Role") != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students.FirstOrDefault(s => s.ID_no == updatedStudent.ID_no);
            if (student == null)
            {
                return NotFound();
            }

            student.full_name = updatedStudent.full_name;
            student.email = updatedStudent.email;
            student.contact = updatedStudent.contact;
            student.grade_section = updatedStudent.grade_section;
            student.IsActive = updatedStudent.IsActive;

            _context.SaveChanges();

            TempData["Success"] = "Student account updated successfully.";
            return RedirectToAction("Index");
        }
    }
}