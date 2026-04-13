using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class Guest
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [EmailAddress]
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}