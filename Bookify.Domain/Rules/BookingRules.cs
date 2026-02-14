using Bookify.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Domain.Rules
{
    public static class BookingRules
    {
        public static bool IsInFuture(DateTime date, TimeSpan time)
        {
            var bookingDateTime = date.Date + time;
            return bookingDateTime > DateTime.UtcNow;
        }

        public static bool CanCancel(BookingStatus status)
        {
            return status is
                BookingStatus.Pending or
                BookingStatus.Approved;
        }

        public static bool CanConfirm(BookingStatus status)
        {
            return status == BookingStatus.Pending;
        }

        public static bool CanComplete(BookingStatus status)
        {
            return status == BookingStatus.Approved;
        }
    }
}
