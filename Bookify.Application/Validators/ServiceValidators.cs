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
                .WithMessage("Service name is required")
                .MinimumLength(5)
                .WithMessage("Service name must be at least 5 characters")
                .MaximumLength(100)
                .WithMessage("Service name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .MinimumLength(10)
                .WithMessage("Description must be at least 10 characters")
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than zero")
                .LessThanOrEqualTo(100000)
                .WithMessage("Price cannot exceed 100,000");

            RuleFor(x => x.Duration)
                .GreaterThanOrEqualTo(30)
                .WithMessage("Duration must be at least 30 minutes")
                .LessThanOrEqualTo(480)
                .WithMessage("Duration cannot exceed 480 minutes (8 hours)");

            RuleFor(x => x.StaffId)
                .NotEmpty()
                .WithMessage("Staff is required");

            RuleFor(x => x.CategoryId)
                .NotEmpty()
                .WithMessage("Category is required");
        }
    }

    public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
    {
        public UpdateServiceRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Service ID is required");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Service name is required")
                .MinimumLength(5)
                .WithMessage("Service name must be at least 5 characters")
                .MaximumLength(100)
                .WithMessage("Service name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .MinimumLength(10)
                .WithMessage("Description must be at least 10 characters")
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than zero")
                .LessThanOrEqualTo(100000)
                .WithMessage("Price cannot exceed 100,000");

            RuleFor(x => x.Duration)
                .GreaterThanOrEqualTo(30)
                .WithMessage("Duration must be at least 30 minutes")
                .LessThanOrEqualTo(480)
                .WithMessage("Duration cannot exceed 480 minutes (8 hours)");
        }
    }
}
