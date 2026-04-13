namespace HotelManagementSystem.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
    }
}