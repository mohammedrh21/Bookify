using System.ComponentModel.DataAnnotations;

namespace Bookify.Domain.Entities
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ServiceId { get; set; }
        public Service Service { get; set; } = default!;

        [Required]
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = default!;

        [Required]
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = default!;

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
