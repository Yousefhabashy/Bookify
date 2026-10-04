using Bookify.Domain.Apartments;
using FluentAssertions;

namespace Domain.UnitTests.Apartments
{
    public class DescriptionTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenDescriptionIsValid()
        {
            var result = Description.Create("Quiet place near the metro.");

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be("Quiet place near the metro.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldFail_WhenDescriptionIsEmptyOrWhitespace(string description)
        {
            var result = Description.Create(description);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Description.Empty");
        }

        [Fact]
        public void Create_ShouldAccept_ExactlyTheMaximumLength()
        {
            Description.Create(new string('a', 2000)).IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldFail_WhenDescriptionIsLongerThanTheMaximum()
        {
            var result = Description.Create(new string('a', 2001));

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Description.TooLong");
        }
    }
}
