using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Guest>? Guests { get; set; }
        public DbSet<Room>? Rooms { get; set; }
        public DbSet<Reservation>? Reservations { get; set; }
        public DbSet<Transaction>? Transactions { get; set; }
    }
}