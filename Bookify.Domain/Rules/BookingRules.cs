using Bookify.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Domain.Rules
{ /// <summary>
  /// Business rules for booking operations
  /// </summary>
    public static class BookingRules
    {
        /// <summary>
        /// Checks if the booking date/time is in the future
        /// </summary>
        /// <param name="date">Booking date</param>
        /// <param name="time">Booking time</param>
        /// <returns>True if booking is in the future, false otherwise</returns>
        public static bool IsInFuture(DateTime date, TimeSpan time)
        {
            var bookingDateTime = date.Date + time;
            return bookingDateTime > DateTime.UtcNow;
        }

        /// <summary>
        /// Validates that a booking time is at least a minimum duration in the future
        /// </summary>
        /// <param name="date">Booking date</param>
        /// <param name="time">Booking time</param>
        /// <param name="minimumHoursAhead">Minimum hours the booking must be ahead (default 1)</param>
        /// <returns>True if booking meets minimum advance booking requirement</returns>
        public static bool MeetsMinimumAdvanceBooking(DateTime date, TimeSpan time, int minimumHoursAhead = 1)
        {
            var bookingDateTime = date.Date + time;
            var minimumDateTime = DateTime.UtcNow.AddHours(minimumHoursAhead);
            return bookingDateTime >= minimumDateTime;
        }

        /// <summary>
        /// Checks if a booking can be cancelled based on its status
        /// </summary>
        public static bool CanCancel(BookingStatus status)
        {
            return status is
                BookingStatus.Pending or
                BookingStatus.Approved;
        }

        /// <summary>
        /// Checks if a booking can be confirmed based on its status
        /// </summary>
        public static bool CanConfirm(BookingStatus status)
        {
            return status == BookingStatus.Pending;
        }

        /// <summary>
        /// Checks if a booking can be completed based on its status
        /// </summary>
        public static bool CanComplete(BookingStatus status)
        {
            return status == BookingStatus.Approved;
        }

        /// <summary>
        /// Checks if a booking can be modified based on its status
        /// </summary>
        public static bool CanModify(BookingStatus status)
        {
            return status is BookingStatus.Pending or BookingStatus.Approved;
        }

        /// <summary>
        /// Validates that the booking time falls within business hours
        /// </summary>
        public static bool IsWithinBusinessHours(TimeSpan time, TimeSpan openTime, TimeSpan closeTime)
        {
            return time >= openTime && time <= closeTime;
        }
    }
}
