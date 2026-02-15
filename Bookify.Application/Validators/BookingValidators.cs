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
                .WithMessage("Service is required");

            RuleFor(x => x.StaffId)
                .NotEmpty()
                .WithMessage("Staff is required");

            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client is required");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Booking date is required")
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("Booking date must be today or in the future");

            RuleFor(x => x.Time)
                .NotEmpty()
                .WithMessage("Booking time is required")
                .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
                .WithMessage("Time must be a valid time of day (00:00-23:59)");

            RuleFor(x => new { x.Date, x.Time })
                .Must(x =>
                {
                    var bookingDateTime = x.Date.Date + x.Time;
                    return bookingDateTime > DateTime.UtcNow;
                })
                .WithMessage("Booking must be in the future")
                .When(x => x.Date >= DateTime.Today);
        }
    }

    public class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
    {
        public CancelBookingRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .NotEmpty()
                .WithMessage("Booking ID is required");

            RuleFor(x => x.RequesterId)
                .NotEmpty()
                .WithMessage("Requester ID is required");

            RuleFor(x => x.RequesterType)
                .IsInEnum()
                .WithMessage("Invalid requester type");
        }
    }
}

