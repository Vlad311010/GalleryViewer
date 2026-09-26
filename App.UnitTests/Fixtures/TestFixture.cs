using AutoFixture;

namespace App.UnitTests.Fixtures
{
    public class TestFixture : Fixture
    {
        public TestFixture()
        {
            Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(Remove);

            Behaviors.Add(new OmitOnRecursionBehavior());
        }

        private void Remove(ThrowingRecursionBehavior behavior)
            => Behaviors.Remove(behavior);
    }
}
