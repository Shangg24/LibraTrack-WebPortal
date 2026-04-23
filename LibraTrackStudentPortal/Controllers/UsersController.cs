using System;
using LibraTrackStudentPortal.Data;
using LibraTrackStudentPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LibraTrackStudentPortal.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            int pageSize = 20;

            var totalAccounts = _context.users.Count(u => u.role == "Librarian");

            var librarians = _context.users
                .Where(u => u.role == "Librarian")
                .OrderBy(u => u.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalAccounts / pageSize);

            return View(librarians);
        }

        public IActionResult Activate(int id)
        {
            var sessionRole = HttpContext.Session.GetString("Role");

            if (sessionRole != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.users.FirstOrDefault(u => u.id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.status = "Active";
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Deactivate(int id)
        {
            var sessionRole = HttpContext.Session.GetString("Role");

            if (sessionRole != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.users.FirstOrDefault(u => u.id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.status = "Deactivated";
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Create(Users user, string plainPassword)
        {
            var sessionRole = HttpContext.Session.GetString("Role");

            if (sessionRole != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(user.id_number) ||
                string.IsNullOrWhiteSpace(user.full_name) ||
                string.IsNullOrWhiteSpace(user.email) ||
                string.IsNullOrWhiteSpace(user.username) ||
                string.IsNullOrWhiteSpace(plainPassword))
            {
                ViewBag.Error = "All fields are required.";
                return View(user);
            }

            var existingUser = _context.users.FirstOrDefault(u =>
                u.username == user.username ||
                u.email == user.email ||
                u.id_number == user.id_number);

            if (existingUser != null)
            {
                ViewBag.Error = "ID Number, Email, or Username already exists.";
                return View(user);
            }

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Users>();

            user.id = _context.users.Any() ? _context.users.Max(u => u.id) + 1 : 1;
            user.password = hasher.HashPassword(user, plainPassword);
            user.role = "Librarian";
            user.status = "Active";
            user.date_register = DateTime.Now;
            user.IsFirstLogin = false;

            _context.users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ResetPassword(int id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.users.FirstOrDefault(u => u.id == id);

            if (user == null)
            {
                return NotFound();
            }

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Users>();
            string newPassword = "admin123";

            user.password = hasher.HashPassword(user, newPassword);

            _context.SaveChanges();

            TempData["Success"] = $"Password reset successful. Temporary password: {newPassword}";

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Role") != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.users.FirstOrDefault(u => u.id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(Users updatedUser)
        {
            if (HttpContext.Session.GetString("Role") != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.users.FirstOrDefault(u => u.id == updatedUser.id);

            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(updatedUser.id_number) ||
                string.IsNullOrWhiteSpace(updatedUser.email) ||
                string.IsNullOrWhiteSpace(updatedUser.username))
            {
                ViewBag.Error = "Please complete all required fields.";
                return View(updatedUser);
            }

            user.full_name = updatedUser.full_name;
            user.email = updatedUser.email;
            user.username = updatedUser.username;

            _context.SaveChanges();

            TempData["Success"] = "Librarian account updated successfully.";
            return RedirectToAction("Index");
        }

    }
}