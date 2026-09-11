using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class AssetGroupQueryValidator : AbstractValidator<AssetGroupQuery>
    {
        public AssetGroupQueryValidator()
        {
            RuleFor(x => x.GroupId).EntityId();
        }
    }
}
