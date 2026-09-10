using FluentValidation;

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
    }
}
