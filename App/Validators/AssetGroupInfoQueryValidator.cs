using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class AssetGroupInfoQueryValidator : AbstractValidator<AssetGroupInfoQuery>
    {
        public AssetGroupInfoQueryValidator()
        {
            RuleFor(x => x.AssetId).EntityId();
        }
    }
}
