using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using LibraTrackStudentPortal.Data;
using LibraTrackStudentPortal.Models;
using System.Collections.Generic;

namespace LibraTrackStudentPortal.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequestsController(ApplicationDbContext context)
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

            var requestRows = (from r in _context.book_requests
                               join b in _context.books on r.book_id equals b.id
                               where r.request_no != null
                               select new
                               {
                                   r.request_id,
                                   RequestNo = r.request_no ?? "",
                                   StudentID = r.ID_no ?? "",
                                   RequestDate = r.request_date,
                                   Status = r.status ?? "",
                                   BookId = b.id ?? "",
                                   BookTitle = b.book_title ?? "",
                                   Author = b.author ?? ""
                               }).ToList();

            var groupedRequests = requestRows
                .GroupBy(x => new { x.RequestNo, x.StudentID, x.RequestDate, x.Status })
                .Select(g => new RequestTransactionViewModel
                {
                    RequestNo = g.Key.RequestNo,
                    StudentID = g.Key.StudentID,
                    RequestDate = g.Key.RequestDate,
                    Status = g.Key.Status,
                    BookCount = g.Count(),
                    Books = g.Select(x => new RequestBookDetail
                    {
                        RequestId = x.request_id,
                        BookId = x.BookId,
                        BookTitle = x.BookTitle,
                        Author = x.Author
                    }).ToList()
                })
                .OrderByDescending(x => x.RequestDate)
                .ToList();

            ViewBag.TotalRequests = groupedRequests.Count;
            ViewBag.PendingRequests = groupedRequests.Count(r => r.Status == "Pending");
            ViewBag.ReservedRequests = groupedRequests.Count(r => r.Status == "Reserved");
            ViewBag.BorrowedRequests = groupedRequests.Count(r => r.Status == "Borrowed");
            ViewBag.CompletedRequests = groupedRequests.Count(r => r.Status == "Completed");
            ViewBag.RejectedRequests = groupedRequests.Count(r => r.Status == "Rejected");

            int pageSize = 20;
            int totalRequests = groupedRequests.Count;

            var pagedRequests = groupedRequests
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRequests / pageSize);

            return View(pagedRequests);
        }

        public IActionResult Approve(string requestNo)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = _context.book_requests
                .Where(r => r.request_no == requestNo)
                .ToList();

            if (!requests.Any())
            {
                return NotFound();
            }

            foreach (var request in requests)
            {
                request.status = "Reserved";
            }

            _context.SaveChanges();

            TempData["Success"] = $"Request {requestNo} approved successfully.";
            return RedirectToAction("Index");
        }

        public IActionResult Reject(string requestNo)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = _context.book_requests
                .Where(r => r.request_no == requestNo)
                .ToList();

            if (!requests.Any())
            {
                return NotFound();
            }

            foreach (var request in requests)
            {
                request.status = "Rejected";
            }

            _context.SaveChanges();

            TempData["Success"] = $"Request {requestNo} rejected successfully.";
            return RedirectToAction("Index");
        }
    }
}