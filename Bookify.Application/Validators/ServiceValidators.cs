using Bookify.Application.DTO.Service;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Validators
{
    public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
    {
        public CreateServiceRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("Service name is required.")
                .MinimumLength(3)
                    .WithMessage("Service name must be at least 3 characters.")
                .MaximumLength(100)
                    .WithMessage("Service name cannot exceed 100 characters.")
                .Matches(@"^[a-zA-Z0-9\s\-&']+$")
                    .WithMessage("Service name contains invalid characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                    .WithMessage("Description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Price)
                .GreaterThan(0)
                    .WithMessage("Price must be greater than zero.")
                .LessThanOrEqualTo(100_000)
                    .WithMessage("Price cannot exceed 100,000.")
                .PrecisionScale(10, 2, false)
                    .WithMessage("Price must have at most 2 decimal places.");

            RuleFor(x => x.Duration)
                .InclusiveBetween(30, 480)
                    .WithMessage("Duration must be between 30 and 480 minutes (8 hours).");

            RuleFor(x => x.StaffId)
                .NotEmpty()
                    .WithMessage("Staff ID is required.");

            RuleFor(x => x.CategoryId)
                .NotEmpty()
                    .WithMessage("Category ID is required.");
        }
    }

    public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
    {
        public UpdateServiceRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                    .WithMessage("Service ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("Service name is required.")
                .MinimumLength(3)
                    .WithMessage("Service name must be at least 3 characters.")
                .MaximumLength(100)
                    .WithMessage("Service name cannot exceed 100 characters.")
                .Matches(@"^[a-zA-Z0-9\s\-&']+$")
                    .WithMessage("Service name contains invalid characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                    .WithMessage("Description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Price)
                .GreaterThan(0)
                    .WithMessage("Price must be greater than zero.")
                .LessThanOrEqualTo(100_000)
                    .WithMessage("Price cannot exceed 100,000.")
                .PrecisionScale(10, 2, false)
                    .WithMessage("Price must have at most 2 decimal places.");

            RuleFor(x => x.Duration)
                .InclusiveBetween(30, 480)
                    .WithMessage("Duration must be between 30 and 480 minutes (8 hours).");
        }
    }
}

