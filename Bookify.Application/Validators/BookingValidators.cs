using Bookify.Application.DTO.Booking;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Validators
{
    public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
    {
        public CreateBookingRequestValidator()
        {
            RuleFor(x => x.ServiceId)
                .NotEmpty()
                    .WithMessage("Service ID is required.");

            RuleFor(x => x.StaffId)
                .NotEmpty()
                    .WithMessage("Staff ID is required.");

            RuleFor(x => x.ClientId)
                .NotEmpty()
                    .WithMessage("Client ID is required.");

            RuleFor(x => x.Date)
                .NotEmpty()
                    .WithMessage("Booking date is required.")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                    .WithMessage("Booking date cannot be in the past.");

            RuleFor(x => x.Time)
                .NotEmpty()
                    .WithMessage("Booking time is required.")
                .Must(t => t >= TimeSpan.Zero && t < TimeSpan.FromHours(24))
                    .WithMessage("Time must be a valid time of day (00:00 – 23:59).")
                .Must(t => t.Minutes % 15 == 0)
                    .WithMessage("Bookings must start on a 15-minute boundary (e.g. 09:00, 09:15).");

            // Combined future-datetime check
            RuleFor(x => new { x.Date, x.Time })
                .Must(x => x.Date.Date + x.Time > DateTime.UtcNow)
                    .WithMessage("Booking must be scheduled in the future.")
                .When(x => x.Date >= DateTime.UtcNow.Date);
        }
    }

    public class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
    {
        public CancelBookingRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .NotEmpty()
                    .WithMessage("Booking ID is required.");

            RuleFor(x => x.RequesterId)
                .NotEmpty()
                    .WithMessage("Requester ID is required.");

            RuleFor(x => x.RequesterType)
                .IsInEnum()
                    .WithMessage("Invalid requester type.");
        }
    }
}
