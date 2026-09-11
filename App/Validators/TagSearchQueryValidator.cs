using App.Models.Queries;
using FluentValidation;

namespace App.Validators
{
    internal class TagSearchQueryValidator : AbstractValidator<TagSearchQuery>
    {
        public TagSearchQueryValidator()
        {
            RuleFor(x => x.SearchKey)
                .NotEmpty()
                .Matches(@"^\s*\S+\s*$") // single word (leading/trailing spaces allowed)
                .WithMessage($"Search key must be a single word.");

            RuleFor(x => x.Take).GreaterThan(0);
        }
    }
}
