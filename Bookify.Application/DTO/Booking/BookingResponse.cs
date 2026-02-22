namespace Bookify.Application.DTO.Booking
{
    public class BookingResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public Guid ClientId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Status { get; set; } = default!;

        // Computed / joined fields for display
        public string ServiceName { get; set; } = default!;
        public string StaffName { get; set; } = default!;
        public string ClientName { get; set; } = default!;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
    }
}
