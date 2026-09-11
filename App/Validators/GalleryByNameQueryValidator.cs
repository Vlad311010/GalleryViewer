using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class GalleryByNameQueryValidator : AbstractValidator<GalleryByNameQuery>
    {
        public GalleryByNameQueryValidator()
        {
            RuleFor(x => x.GalleryName).NotEmpty();
        }
    }
}
