using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class PhysicalAssetGroupQueryValidator : AbstractValidator<PhysicalAssetGroupQuery>
    {
        public PhysicalAssetGroupQueryValidator()
        {
            RuleFor(x => x.GalleryId).EntityId();

            RuleFor(x => x.PhysicalPath).NotEmpty().RelativePath();
        }
    }
}
