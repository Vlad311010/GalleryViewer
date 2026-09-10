using App.Commands;
using FluentValidation;

namespace App.Validators
{
    internal class SetGroupCoverCommandValidator : AbstractValidator<SetGroupCoverCommand>
    {
        public SetGroupCoverCommandValidator()
        {
            RuleFor(x => x.AssetId).EntityId();

            RuleFor(x => x.GroupId).EntityId();
        }
    }
}
