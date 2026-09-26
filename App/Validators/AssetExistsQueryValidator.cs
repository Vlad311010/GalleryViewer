using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class AssetExistsQueryValidator : AbstractValidator<AssetExistsQuery>
    {
        public AssetExistsQueryValidator()
        {
            RuleFor(x => x.GalleryId).EntityId();

            RuleFor(x => x.RelativePath).NotEmpty().RelativePath();
        }
    }
}
