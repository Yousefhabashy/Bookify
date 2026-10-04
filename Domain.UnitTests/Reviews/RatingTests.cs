using Bookify.Domain.Reviews;
using FluentAssertions;

namespace Domain.UnitTests.Reviews
{
    public class RatingTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void Create_ShouldSucceed_WhenValueIsBetweenOneAndFive(int value)
        {
            var result = Rating.Create(value);

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        [InlineData(100)]
        public void Create_ShouldFail_WhenValueIsOutsideTheRange(int value)
        {
            var result = Rating.Create(value);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(Rating.Invalid);
        }
    }
}
