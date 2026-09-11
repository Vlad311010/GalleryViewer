using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class AssetTagsQueryValidator : AbstractValidator<AssetTagsQuery>
    {
        public AssetTagsQueryValidator()
        {
            RuleFor(x => x.AssetId).EntityId();
        }
    }
}
