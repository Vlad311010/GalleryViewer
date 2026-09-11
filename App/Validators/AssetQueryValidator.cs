using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class AssetQueryValidator : AbstractValidator<AssetQuery>
    {
        public AssetQueryValidator()
        {
            RuleFor(x => x.AssetId).EntityId();
        }
    }
}
