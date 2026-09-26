using App.Models.Dtos.Media;
using FluentValidation;

namespace App.Validators
{
    internal class PreviewQueryValidator : AbstractValidator<PreviewQuery>
    {
        public PreviewQueryValidator()
        {
            RuleFor(x => x.ItemId).EntityId();

            RuleFor(x => x.ItemType).IsInEnum();
        }
    }
}
