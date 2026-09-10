using App.Commands;
using FluentValidation;

namespace App.Validators
{
    internal class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
    {
        public CreateTagCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Category)
                .NotEmpty();

            RuleFor(x => x.CanonicalId)
                .EntityId().When(x => x.CanonicalId.HasValue);
        }
    }
}
