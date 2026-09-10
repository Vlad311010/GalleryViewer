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
                .NotEmpty();

            RuleFor(x => x.PreviewPath)
                .NotEmpty();

            RuleFor(x => x.GroupId)
                .EntityId();

            RuleFor(x => x.GroupPosition)
                .GreaterThan(0)
                .When(x => x.GroupPosition.HasValue);
        }
    }
}
