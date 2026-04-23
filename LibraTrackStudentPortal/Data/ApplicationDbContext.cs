using Microsoft.EntityFrameworkCore;
using LibraTrackStudentPortal.Models;

namespace LibraTrackStudentPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<issue> issues { get; set; }
        public DbSet<issue_books> issue_books { get; set; }
        public DbSet<book> books { get; set; }
        public DbSet<book_requests> book_requests { get; set; }
        public DbSet<Users> users { get; set; }
        public DbSet<GradeSection> GradeSections { get; set; }

    }

}
