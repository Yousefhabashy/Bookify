using Bookify.Domain.Users;
using FluentAssertions;

namespace Domain.UnitTests.Users
{
    public class LastNameTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenValueIsValid()
        {
            var result = LastName.Create("Alex");

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be("Alex");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldFail_WhenValueIsEmptyOrWhitespace(string value)
        {
            var result = LastName.Create(value);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("LastName.Empty");
        }

        [Fact]
        public void Create_ShouldAccept_ExactlyTheMaximumLength()
        {
            LastName.Create(new string('a', 100)).IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldFail_WhenValueIsLongerThanTheMaximum()
        {
            var result = LastName.Create(new string('a', 101));

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("LastName.TooLong");
        }
    }
}
