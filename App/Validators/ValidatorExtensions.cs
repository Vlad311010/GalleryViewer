using App.Models;
using FluentValidation;
using System.Text.RegularExpressions;

namespace App.Validators
{
    internal static class ValidatorExtensions
    {
        public static IRuleBuilderOptions<T, int> EntityId<T>(this IRuleBuilder<T, int> rule)
        {
            return rule.GreaterThan(0);
        }

        public static IRuleBuilderOptions<T, int?> EntityId<T>(this IRuleBuilder<T, int?> rule)
        {
            return rule.GreaterThan(0);
        }

        public static IRuleBuilderOptions<T, string> Tag<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty()
                .Matches(@"^[^-:_\}\{\]\[]+$");
        }

        public static IRuleBuilderOptions<T, Pagination> Pagination<T>(this IRuleBuilder<T, Pagination> rule)
        {
            return rule.ChildRules(pagination =>
            {
                pagination.RuleFor(x => x.Skip)
                    .GreaterThanOrEqualTo(0);

                pagination.RuleFor(x => x.Take)
                    .GreaterThan(0);
            });
        }

        public static IRuleBuilderOptions<T, TagFilters> TagFilters<T>(this IRuleBuilder<T, TagFilters> rule)
        {
            return rule.ChildRules(filters =>
            {
                filters.RuleForEach(x => x.Tags)
                    .Tag();

                filters.RuleForEach(x => x.ExcludeTags)
                    .Tag();
            });
        }

        public static IRuleBuilderOptions<T, string?> RelativePath<T>(this IRuleBuilder<T, string?> rule)
        {
            return rule
                .Must(path =>
                    path != null &&
                    !path.StartsWith('/') &&
                    !path.Contains('\\') &&
                    !Path.IsPathRooted(path) &&
                    !path.Split('/').Any(x => x == "" || x == ".."))
                .WithMessage("Path must be a valid relative path.");
        }

        public static IRuleBuilderOptions<T, string?> AbsolutePath<T>(this IRuleBuilder<T, string?> rule)
        {
            return rule
                .Must(path =>
                    path != null &&
                    !path.Contains('\0') &&
                    (path.StartsWith('/') ||
                     Regex.IsMatch(path, @"^[A-Za-z]:[\\/]")))
                .WithMessage("Path must be an absolute path.");
        }
    }
}
