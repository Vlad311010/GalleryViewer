using App.Models.Commands;
using FluentValidation;

namespace App.Validators
{
    internal class AssetRemoveTagCommandValidator : AbstractValidator<AssetRemoveTagCommand>
    {
        public AssetRemoveTagCommandValidator()
        {
            RuleFor(x => x.AssetId).EntityId();

            RuleFor(x => x.Tag).Tag();
        }
    }
}
