using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class Room
    {
        public int Id { get; set; }
        [Required]
        public string Number { get; set; } = string.Empty;
        public string? Type { get; set; }
        public decimal Rate { get; set; }
        public bool IsOccupied { get; set; }
    }
}