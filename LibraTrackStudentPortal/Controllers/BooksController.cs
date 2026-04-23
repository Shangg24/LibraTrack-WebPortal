using System;
using System.Collections.Generic;
using LibraTrackStudentPortal.Data;
using LibraTrackStudentPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index(int page = 1)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            int pageSize = 20;

            var allBooks = _context.books.ToList();

            ViewBag.TotalBooks = allBooks.Count;
            ViewBag.AvailableBooks = allBooks.Count(b => b.available > 0);
            ViewBag.UnavailableBooks = allBooks.Count(b => b.available <= 0);

            var pagedBooks = allBooks
                .OrderBy(b => b.book_title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)allBooks.Count / pageSize);

            return View(pagedBooks);
        }

        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Categories = GetBookCategories();

            return View();
        }

        [HttpPost]
        public IActionResult Create(book book)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(book.book_title) ||
                string.IsNullOrWhiteSpace(book.author) ||
                string.IsNullOrWhiteSpace(book.category) ||
                string.IsNullOrWhiteSpace(book.ISBN) ||
                string.IsNullOrWhiteSpace(book.shelf))
            {
                ViewBag.Error = "Please complete all required fields.";
                ViewBag.Categories = GetBookCategories();
                return View(book);
            }

            if (book.Copies < 0)
            {
                ViewBag.Error = "Copies cannot be negative.";
                ViewBag.Categories = GetBookCategories();
                return View(book);
            }

            var lastBookId = _context.books
                .Select(b => b.id)
                .Where(id => id != null && id.StartsWith("B-"))
                .OrderByDescending(id => id)
                .FirstOrDefault();

            string newBookId = "B-0001";

            if (!string.IsNullOrWhiteSpace(lastBookId))
            {
                var numericPart = lastBookId.Replace("B-", "");
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    newBookId = "B-" + (lastNumber + 1).ToString("D4");
                }
            }

            book.id = newBookId;
            book.available = book.Copies;
            book.status = book.available > 0 ? "Available" : "Not Available";
            book.date_insert = DateTime.Now;

            _context.books.Add(book);
            _context.SaveChanges();

            TempData["Success"] = "Book added successfully.";
            return RedirectToAction("Index");
        }

        private List<string> GetBookCategories()
        {
            return new List<string>
            {
                "000-099: General Works",
                "100-199: Philosophy and Psychology",
                "200-299: Religion",
                "300-399: Social Sciences",
                "400-499: Language",
                "500-599: Science and Mathematics",
                "600-699: Technology",
                "700-799: Arts and Recreation",
                "800-899: Literature",
                "900-999: History and Geography"
            };
        }

        public IActionResult Edit(string id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var book = _context.books.FirstOrDefault(b => b.id == id);

            if (book == null)
            {
                return NotFound();
            }

            ViewBag.Categories = GetBookCategories();
            return View(book);
        }

        [HttpPost]
        public IActionResult Edit(book updatedBook)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var book = _context.books.FirstOrDefault(b => b.id == updatedBook.id);

            if (book == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(updatedBook.book_title) ||
                string.IsNullOrWhiteSpace(updatedBook.author) ||
                string.IsNullOrWhiteSpace(updatedBook.category) ||
                string.IsNullOrWhiteSpace(updatedBook.ISBN) ||
                string.IsNullOrWhiteSpace(updatedBook.shelf))
            {
                ViewBag.Error = "Please complete all required fields.";
                ViewBag.Categories = GetBookCategories();
                return View(updatedBook);
            }

            if (updatedBook.Copies < 0)
            {
                ViewBag.Error = "Copies cannot be negative.";
                ViewBag.Categories = GetBookCategories();
                return View(updatedBook);
            }

            // Calculate currently borrowed copies
            int currentCopies = book.Copies ?? 0;
            int currentAvailable = book.available ?? 0;
            int borrowedCopies = currentCopies - currentAvailable;

            if (borrowedCopies < 0)
            {
                borrowedCopies = 0;
            }

            // Prevent setting total copies below borrowed copies
            if ((updatedBook.Copies ?? 0) < borrowedCopies)
            {
                ViewBag.Error = $"Total copies cannot be less than the currently borrowed copies ({borrowedCopies}).";
                ViewBag.Categories = GetBookCategories();
                return View(updatedBook);
            }

            // Update book details
            book.book_title = updatedBook.book_title;
            book.author = updatedBook.author;
            book.published_date = updatedBook.published_date;
            book.category = updatedBook.category;
            book.ISBN = updatedBook.ISBN;
            book.shelf = updatedBook.shelf;
            book.image = updatedBook.image;

            // Safely adjust copies
            book.Copies = updatedBook.Copies;
            book.available = (updatedBook.Copies ?? 0) - borrowedCopies;

            // Auto-update status
            book.status = (book.available ?? 0) > 0 ? "Available" : "Not Available";
            book.date_update = DateTime.Now;

            _context.SaveChanges();

            TempData["Success"] = "Book updated successfully.";
            return RedirectToAction("Index");
        }
    }

}
