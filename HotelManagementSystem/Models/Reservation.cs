using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public enum ReservationStatus { Booked, CheckedIn, CheckedOut, Cancelled }

    public class Reservation
    {
        public int Id { get; set; }
        [Required]
        public int GuestId { get; set; }
        public Guest? Guest { get; set; }
        [Required]
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal Total { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Booked;
    }
}