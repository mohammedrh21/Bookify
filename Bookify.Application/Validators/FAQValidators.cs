using Bookify.Application.DTO.FAQ;
using FluentValidation;

namespace Bookify.Application.Validators
{
    public class CreateFAQRequestValidator : AbstractValidator<CreateFAQRequest>
    {
        public CreateFAQRequestValidator()
        {
            RuleFor(x => x.Question)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.Answer)
                .NotEmpty()
                .MaximumLength(2000);
        }
    }

    public class UpdateFAQRequestValidator : AbstractValidator<UpdateFAQRequest>
    {
        public UpdateFAQRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Question)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.Answer)
                .NotEmpty()
                .MaximumLength(2000);
        }
    }
}
