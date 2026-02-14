using Bookify.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.DTO.Booking
{
    public class CreateBookingRequest
    {
        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public Guid StaffId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }
    }
}
