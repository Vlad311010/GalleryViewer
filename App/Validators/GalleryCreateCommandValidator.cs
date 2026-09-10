using App.Commands;
using FluentValidation;

namespace App.Validators
{
    internal class GalleryCreateCommandValidator : AbstractValidator<GalleryCreateCommand>
    {
        public GalleryCreateCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Path).NotEmpty();
        }
    }
}
