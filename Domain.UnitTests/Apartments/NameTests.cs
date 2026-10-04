using Bookify.Domain.Apartments;
using FluentAssertions;

namespace Domain.UnitTests.Apartments
{
    public class NameTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenNameIsValid()
        {
            var result = Name.Create("Sea View Apartment");

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be("Sea View Apartment");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldFail_WhenNameIsEmptyOrWhitespace(string name)
        {
            var result = Name.Create(name);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Name.Empty");
        }

        [Fact]
        public void Create_ShouldFail_WhenNameIsNull()
        {
            var result = Name.Create(null!);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Name.Empty");
        }

        [Fact]
        public void Create_ShouldAccept_ExactlyTheMaximumLength()
        {
            Name.Create(new string('a', 200)).IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldFail_WhenNameIsLongerThanTheMaximum()
        {
            var result = Name.Create(new string('a', 201));

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Name.TooLong");
        }
    }
}
