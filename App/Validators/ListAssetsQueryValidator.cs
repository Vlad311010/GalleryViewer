using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class ListAssetsQueryValidator : AbstractValidator<ListAssetsQuery>
    {
        public ListAssetsQueryValidator()
        {
            RuleFor(x => x.GalleryName).NotEmpty();

            RuleFor(x => x.Pagination).Pagination();

            RuleFor(x => x.TagFilters).TagFilters();
        }
    }
}
