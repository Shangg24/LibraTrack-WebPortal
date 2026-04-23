using System;
using LibraTrackStudentPortal.Data;
using LibraTrackStudentPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LibraTrackStudentPortal.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int alertsPage = 1)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Librarian" && role != "IT")
            {
                return RedirectToAction("Login", "Account");
            }

            var totalBorrowedTransactions = _context.issues.Count();
            var totalRequestedBooks = _context.book_requests.Count();
            var lowStockBooks = _context.books.Count(b => (b.available ?? 0) <= 2);

            var mostBorrowedCategory = (from ib in _context.issue_books
                                        join b in _context.books on ib.book_id equals b.id
                                        group b by b.category into g
                                        orderby g.Count() descending
                                        select g.Key).FirstOrDefault() ?? "No data";

            var topBorrowedBooks = (from ib in _context.issue_books
                                    join b in _context.books on ib.book_id equals b.id
                                    group b by new { b.id, b.book_title } into g
                                    orderby g.Count() descending
                                    select new TopBookViewModel
                                    {
                                        BookId = g.Key.id,
                                        BookTitle = g.Key.book_title,
                                        Count = g.Count()
                                    }).Take(5).ToList();

            var topRequestedBooks = (from r in _context.book_requests
                                     join b in _context.books on r.book_id equals b.id
                                     group b by new { b.id, b.book_title } into g
                                     orderby g.Count() descending
                                     select new TopBookViewModel
                                     {
                                         BookId = g.Key.id,
                                         BookTitle = g.Key.book_title,
                                         Count = g.Count()
                                     }).Take(5).ToList();

            var demandAlerts = (from b in _context.books
                                let requestCount = _context.book_requests.Count(r => r.book_id == b.id)
                                let borrowCount = _context.issue_books.Count(ib => ib.book_id == b.id)
                                select new BookDemandViewModel
                                {
                                    BookId = b.id,
                                    BookTitle = b.book_title,
                                    RequestCount = requestCount,
                                    BorrowCount = borrowCount,
                                    AvailableCopies = b.available ?? 0,
                                    DemandLevel =
                                        ((b.available ?? 0) <= 2 && (requestCount + borrowCount) >= 3) ? "High Demand" :
                                        ((b.available ?? 0) <= 5 && (requestCount + borrowCount) >= 2) ? "Monitor" :
                                        "Stable"
                                })
                                .OrderByDescending(x => x.RequestCount + x.BorrowCount)
                                .ToList();

            

            int alertsPageSize = 20;
            int totalAlerts = demandAlerts.Count;

            var pagedDemandAlerts = demandAlerts
                .Skip((alertsPage - 1) * alertsPageSize)
                .Take(alertsPageSize)
                .ToList();

            ViewBag.AlertsPage = alertsPage;
            ViewBag.TotalAlertsPages = (int)Math.Ceiling((double)totalAlerts / alertsPageSize);

            var model = new PredictiveAnalyticsViewModel
            {
                TotalBorrowedTransactions = totalBorrowedTransactions,
                TotalRequestedBooks = totalRequestedBooks,
                LowStockBooks = lowStockBooks,
                MostBorrowedCategory = mostBorrowedCategory,
                TopBorrowedBooks = topBorrowedBooks,
                TopRequestedBooks = topRequestedBooks,
                DemandAlerts = pagedDemandAlerts
            };

            return View(model);
        }
    }
}