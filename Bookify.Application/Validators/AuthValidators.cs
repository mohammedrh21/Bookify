using Bookify.Application.DTO.Auth;
using Bookify.Application.DTO.Identity;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters");
        }
    }

    public class RegisterClientRequestValidator : AbstractValidator<RegisterClientRequest>
    {
        public RegisterClientRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters")
                .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one number");
                

            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("Full name is required")
                .MinimumLength(2)
                .WithMessage("Full name must be at least 2 characters")
                .MaximumLength(50)
                .WithMessage("Full name cannot exceed 50 characters");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Invalid phone number format");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today.AddYears(-13))
                .WithMessage("Must be at least 13 years old")
                .When(x => x.DateOfBirth.HasValue);
        }
    }

    public class RegisterStaffRequestValidator : AbstractValidator<RegisterStaffRequest>
    {
        public RegisterStaffRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters")
                .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one number");

            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("Full name is required")
                .MinimumLength(2)
                .WithMessage("Full name must be at least 2 characters")
                .MaximumLength(50)
                .WithMessage("Full name cannot exceed 50 characters");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Invalid phone number format");
        }
    }

    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token is required");
        }
    }
}
