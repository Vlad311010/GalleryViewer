using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class ListGroupAssetsQueryValidator : AbstractValidator<ListGroupAssetsQuery>
    {
        public ListGroupAssetsQueryValidator()
        {
            RuleFor(x => x.GroupId).EntityId();

            RuleFor(x => x.Pagination).Pagination();
        }
    }
}
