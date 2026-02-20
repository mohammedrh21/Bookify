using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Domain.Exceptions
{
    /// <summary>
    /// Base class for all domain-specific exceptions.
    /// Thrown when a business rule is violated.
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
        protected DomainException(string message, Exception inner) : base(message, inner) { }
    }

    // ──────────────────────────────────────────
    // 404 – Not Found
    // ──────────────────────────────────────────

    /// <summary>Thrown when a requested resource cannot be found.</summary>
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entity, object id)
            : base($"{entity} with id '{id}' was not found.") { }

        public NotFoundException(string message) : base(message) { }
    }

    // ──────────────────────────────────────────
    // 409 – Conflict
    // ──────────────────────────────────────────

    /// <summary>Thrown when an entity already exists (duplicate).</summary>
    public class ConflictException : DomainException
    {
        public ConflictException(string message) : base(message) { }
    }

    // ──────────────────────────────────────────
    // 400 – Validation / Business Rule
    // ──────────────────────────────────────────

    /// <summary>Thrown when a business rule or domain invariant is violated.</summary>
    public class BusinessRuleException : DomainException
    {
        public BusinessRuleException(string message) : base(message) { }
    }

    // ──────────────────────────────────────────
    // 403 – Forbidden
    // ──────────────────────────────────────────

    /// <summary>Thrown when an authenticated user attempts an action they are not permitted to perform.</summary>
    public class ForbiddenException : DomainException
    {
        public ForbiddenException(string message = "You are not authorized to perform this action.")
            : base(message) { }
    }

    // ──────────────────────────────────────────
    // Booking-specific
    // ──────────────────────────────────────────

    /// <summary>Thrown when a booking status transition is invalid.</summary>
    public class InvalidBookingTransitionException : DomainException
    {
        public InvalidBookingTransitionException(string from, string to)
            : base($"Cannot transition booking from '{from}' to '{to}'.") { }
    }

    /// <summary>Thrown when a time slot is already occupied.</summary>
    public class TimeSlotUnavailableException : DomainException
    {
        public TimeSlotUnavailableException(DateTime date, TimeSpan time)
            : base($"The time slot on {date:yyyy-MM-dd} at {time:hh\\:mm} is already booked.") { }
    }
}
