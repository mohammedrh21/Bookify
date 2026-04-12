using Bookify.Application.DTO.Identity;
using FluentValidation;

namespace Bookify.Application.Validators;

public sealed class UpdateAdminProfileRequestValidator : AbstractValidator<UpdateAdminProfileRequest>
{
    public UpdateAdminProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Full name cannot exceed 50 characters.")
            .Matches(@"^[\p{L}\s'\-]+$").WithMessage("Full name may only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("Phone number must be in international format (e.g. +966501234567) with 7–15 digits.");
    }
}

public sealed class UpdateClientProfileRequestValidator : AbstractValidator<UpdateClientProfileRequest>
{
    public UpdateClientProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Full name cannot exceed 50 characters.")
            .Matches(@"^[\p{L}\s'\-]+$").WithMessage("Full name may only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("Phone number must be in international format (e.g. +966501234567) with 7–15 digits.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender specified.")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.AddYears(-13)).WithMessage("You must be at least 13 years old.")
            .GreaterThan(DateTime.UtcNow.AddYears(-120)).WithMessage("Date of birth is not valid.")
            .When(x => x.DateOfBirth.HasValue);
    }
}

public sealed class UpdateStaffProfileRequestValidator : AbstractValidator<UpdateStaffProfileRequest>
{
    public UpdateStaffProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Full name cannot exceed 50 characters.")
            .Matches(@"^[\p{L}\s'\-]+$").WithMessage("Full name may only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("Phone number must be in international format (e.g. +966501234567) with 7–15 digits.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender specified.")
            .When(x => x.Gender.HasValue);
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

public sealed class VerifyRegistrationOtpRequestValidator : AbstractValidator<VerifyRegistrationOtpRequest>
{
    public VerifyRegistrationOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP is required.")
            .Length(6).WithMessage("OTP must be exactly 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("OTP must contain digits only.");
    }
}
