using Bookify.Application.DTO.ContactInfo;
using FluentValidation;

namespace Bookify.Application.Validators
{
    public class CreateContactInfoRequestValidator : AbstractValidator<CreateContactInfoRequest>
    {
        public CreateContactInfoRequestValidator()
        {
            RuleFor(x => x.Country)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.AddressLine_1)
                .MaximumLength(200);

            RuleFor(x => x.AddressLine_2)
                .MaximumLength(200);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(150);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.CallDayFrom)
                .IsInEnum();

            RuleFor(x => x.CallDayTo)
                .IsInEnum();

            RuleFor(x => x.CallHourFrom)
                .NotEmpty();

            RuleFor(x => x.CallHourTo)
                .NotEmpty();
        }
    }

    public class UpdateContactInfoRequestValidator : AbstractValidator<UpdateContactInfoRequest>
    {
        public UpdateContactInfoRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Country)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.AddressLine_1)
                .MaximumLength(200);

            RuleFor(x => x.AddressLine_2)
                .MaximumLength(200);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(150);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.CallDayFrom)
                .IsInEnum();

            RuleFor(x => x.CallDayTo)
                .IsInEnum();

            RuleFor(x => x.CallHourFrom)
                .NotEmpty();

            RuleFor(x => x.CallHourTo)
                .NotEmpty();
        }
    }
}
