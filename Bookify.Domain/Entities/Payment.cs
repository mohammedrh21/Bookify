using Bookify.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Domain.Entities
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = default!;

        public Guid ServiceId { get; set; }
        public Service Service { get; set; } = default!;

        public Guid? BookingId { get; set; }
        public Booking? Booking { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
