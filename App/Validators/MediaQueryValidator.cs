using App.Models.Dtos.Media;
using FluentValidation;

namespace App.Validators
{
    internal class MediaQueryValidator : AbstractValidator<MediaQuery>
    {
        public MediaQueryValidator()
        {
            RuleFor(x => x.ItemId).EntityId();

            RuleFor(x => x.ItemType).IsInEnum();
        }
    }
}
