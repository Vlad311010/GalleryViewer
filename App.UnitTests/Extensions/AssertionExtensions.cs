using FluentValidation;

namespace App.UnitTests.Extensions
{
    public static class AssertionExtensions
    {
        public static void AssertSingleError(this ValidationException exception, string property)
        {
            var error = Assert.Single(exception.Errors);
            var actualProperty = error.PropertyName.Split('[')[0]; // stripping "[N]" index part from array properties

            Assert.Equal(property, actualProperty);
        }
    }
}
