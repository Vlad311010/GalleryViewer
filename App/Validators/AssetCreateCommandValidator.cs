using App.Commands;
using FluentValidation;

namespace App.Validators
{
    internal class AssetCreateCommandValidator : AbstractValidator<AssetCreateCommand>
    {
        public AssetCreateCommandValidator()
        {
            RuleFor(x => x.GalleryId)
                .EntityId();

            RuleFor(x => x.RelativePath)
                .NotEmpty()
                .RelativePath();

            RuleFor(x => x.PreviewPath)
                .AbsolutePath()
                .When(x => string.IsNullOrEmpty(x.PreviewPath));

            RuleFor(x => x.GroupId)
                .EntityId();

            RuleFor(x => x.GroupPosition)
                .GreaterThanOrEqualTo(0)
                .When(x => x.GroupPosition.HasValue);
        }
    }
}
