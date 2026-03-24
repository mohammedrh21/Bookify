using System;

namespace Bookify.Client.Models.Review
{
    public class ReviewModel
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = default!;
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = default!;
        public Guid BookingId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
