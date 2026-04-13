using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
    public class GuestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Guests
        public async Task<IActionResult> Index()
        {
            var guests = await _context.Guests!.ToListAsync();
            return View(guests);
        }

        // GET: Guests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var guest = await _context.Guests!.FirstOrDefaultAsync(g => g.Id == id.Value);
            if (guest == null) return NotFound();

            return View(guest);
        }

        // GET: Guests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Guests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Phone")] Guest guest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(guest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(guest);
        }

        // GET: Guests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var guest = await _context.Guests!.FindAsync(id.Value);
            if (guest == null) return NotFound();
            return View(guest);
        }

        // POST: Guests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Phone")] Guest guest)
        {
            if (id != guest.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(guest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GuestExists(guest.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(guest);
        }

        // GET: Guests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var guest = await _context.Guests!.FirstOrDefaultAsync(g => g.Id == id.Value);
            if (guest == null) return NotFound();

            return View(guest);
        }

        // POST: Guests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var guest = await _context.Guests!.FindAsync(id);
            if (guest != null)
            {
                _context.Guests!.Remove(guest);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GuestExists(int id)
        {
            return _context.Guests!.Any(e => e.Id == id);
        }
    }
}
