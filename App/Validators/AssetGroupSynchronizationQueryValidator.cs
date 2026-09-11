using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class AssetGroupSynchronizationQueryValidator : AbstractValidator<AssetGroupSynchronizationQuery>
    {
        public AssetGroupSynchronizationQueryValidator()
        {
            RuleFor(x => x.GroupId).EntityId();

            RuleFor(x => x.Files).NotNull();

            RuleForEach(x => x.Files).NotEmpty().RelativePath();
        }
    }
}
