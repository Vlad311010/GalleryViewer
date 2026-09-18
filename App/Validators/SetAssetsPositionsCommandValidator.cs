using App.Models.Commands;
using FluentValidation;

namespace App.Validators
{
    internal class SetAssetsPositionsCommandValidator : AbstractValidator<SetAssetsPositionsCommand>
    {
        public SetAssetsPositionsCommandValidator()
        {
            RuleFor(x => x.GroupId).EntityId();

            RuleForEach(x => x.Positions)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.Id)
                        .EntityId();

                    item.RuleFor(x => x.Position)
                        .GreaterThanOrEqualTo(0);
                });

            RuleFor(x => x.Positions)
                .Must(x => x.Select(p => p.Position).Distinct().Count() == x.Count())
                .WithMessage("Positions must be distinct.");
        }
    }
}
