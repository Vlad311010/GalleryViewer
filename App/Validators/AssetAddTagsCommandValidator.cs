using App.Commands;
using FluentValidation;

namespace App.Validators
{
    internal class AssetAddTagsCommandValidator : AbstractValidator<AssetAddTagsCommand>
    {
        public AssetAddTagsCommandValidator()
        {
            RuleFor(x => x.AssetId).EntityId();

            RuleFor(x => x.Tags).NotEmpty();

            RuleForEach(x => x.Tags).Tag();
        }
    }
}
