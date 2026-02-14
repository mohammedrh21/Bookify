using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
