using Bookify.Domain.Enums;
using System.ComponentModel.DataAnnotations;


namespace Bookify.Domain.Entities
{
    /// <summary>
    /// Represents a booking of a service by a client
    /// </summary>
    public class Booking
    {
        [Key]
        public Guid Id { get; set; }

        // Many-to-One: Booking → Service
        public Guid ServiceId { get; set; }
        public Service Service { get; set; } = default!;

        // Many-to-One: Booking → Client
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = default!;

        // Optional: Staff is retrieved via Service.Staff
        // public Guid StaffId { get; set; }
        // public Staff Staff { get; set; } = default!;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}