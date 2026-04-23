using Microsoft.AspNetCore.Mvc;
using LibraTrackStudentPortal.Data;
using System.Linq;
using LibraTrackStudentPortal.Models;
using Microsoft.AspNetCore.Identity;

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
        public IActionResult Login(string username, string password)
        {
            // 🔵 Check Admin first
            // 🔵 DEBUG ADMIN LOGIN (STEP BY STEP)
            var user = _context.users
    .FirstOrDefault(u => u.username != null && u.username.Trim() == username.Trim());

            if (user != null)
            {
                var hasher = new PasswordHasher<Users>();
                var result = hasher.VerifyHashedPassword(user, user.password, password);

                if (result == PasswordVerificationResult.Failed)
                {
                    ViewBag.Error = "Wrong password.";
                    return View();
                }

                if (user.status != "Active")
                {
                    ViewBag.Error = "Your account is deactivated. Please contact the administrator.";
                    return View();
                }

                HttpContext.Session.SetString("Username", user.username);
                HttpContext.Session.SetString("Role", user.role); // VERY IMPORTANT

                // 🔥 Redirect based on role
                if (user.role == "IT")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (user.role == "Librarian")
                {
                    return RedirectToAction("Dashboard", "Librarian");
                }
                else
                {
                    ViewBag.Error = "Role not recognized.";
                    return View();
                }
            }

            // 🟢 Then check Student
            var student = _context.Students
                .FirstOrDefault(s => s.ID_no == username);

            if (student == null)
            {
                ViewBag.Error = "Account not found.";
                return View();
            }

            if (!student.IsActive)
            {
                ViewBag.Error = "Your account has been deactivated.";
                return View();
            }

            if (student.passwordHash != password)
            {
                ViewBag.Error = "Wrong password.";
                return View();
            }

            HttpContext.Session.SetString("Role", "Student");
            HttpContext.Session.SetString("StudentID", student.ID_no);
            HttpContext.Session.SetString("StudentName", student.full_name ?? "");

            if (student.MustChangePassword)
            {
                HttpContext.Session.SetString("MustChangePassword", "true");
                return RedirectToAction("ForceChangePassword", "Student");
            }

            HttpContext.Session.Remove("MustChangePassword");
            return RedirectToAction("Dashboard", "Student");
        }

        public IActionResult ResetMyPassword()
        {
            var username = HttpContext.Session.GetString("Username");

            var user = _context.users.FirstOrDefault(u => u.username == username);

            if (user == null)
            {
                return Content("User not found.");
            }

            var hasher = new PasswordHasher<Users>();
            string newPassword = "admin123";

            user.password = hasher.HashPassword(user, newPassword);

            _context.SaveChanges();

            return Content($"Your password has been reset. New password: {newPassword}");
        }

        public IActionResult ChangePassword()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Success = TempData["Success"];
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.users.FirstOrDefault(u => u.username == username);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var hasher = new PasswordHasher<Users>();
            var result = hasher.VerifyHashedPassword(user, user.password, currentPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "New password and confirm password do not match.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.Error = "New password cannot be empty.";
                return View();
            }

            user.password = hasher.HashPassword(user, newPassword);
            _context.SaveChanges();

            TempData["Success"] = "Password changed successfully.";

            if (role == "IT")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("Dashboard", "Librarian");
            }
        }


    }
}
