using App.Models.Commands;
using FluentValidation;

namespace App.Validators
{
    internal class SetAssetsPositionsCommandValidator : AbstractValidator<SetAssetsPositionsCommand>
    {
        public SetAssetsPositionsCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .GreaterThan(0);

            RuleForEach(x => x.Positions)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.Id)
                        .EntityId();

                    item.RuleFor(x => x.Position)
                        .GreaterThan(0);
                });

            RuleFor(x => x.Positions)
                .Must(x => x.Select(p => p.Position).Distinct().Count() == x.Count())
                .WithMessage("Positions must be distinct.");
        }
    }
}
