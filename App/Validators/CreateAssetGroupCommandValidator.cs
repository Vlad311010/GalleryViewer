using App.Models.Commands;
using FluentValidation;

namespace App.Validators
{
    internal class CreateAssetGroupCommandValidator : AbstractValidator<CreateAssetGroupCommand>
    {
        public CreateAssetGroupCommandValidator()
        {
            RuleFor(x => x.GalleryId).EntityId();

            RuleFor(x => x.GroupName).NotEmpty();

            RuleFor(x => x.PhysicalPath).NotEmpty().RelativePath();

            RuleFor(x => x.CreationTimeOverride).NotEmpty();
        }
    }
}
