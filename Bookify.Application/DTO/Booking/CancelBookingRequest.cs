using Bookify.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.Booking
{
    public class CancelBookingRequest
    {
        public Guid BookingId { get; set; }
        public Guid RequesterId { get; set; }

        /// <summary>
        /// Client or Staff (used only for business validation)
        /// Authorization should still be enforced by policies
        /// </summary>
        public BookingRequesterType RequesterType { get; set; }
    }
}
