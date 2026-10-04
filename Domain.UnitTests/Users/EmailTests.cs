using Bookify.Domain.Users;
using FluentAssertions;

namespace Domain.UnitTests.Users
{
    public class EmailTests
    {
        [Theory]
        [InlineData("john.doe@example.com")]
        [InlineData("a@b.co")]
        [InlineData("first+tag@sub.domain.org")]
        public void Create_ShouldSucceed_WhenEmailIsValid(string email)
        {
            var result = Email.Create(email);

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(email);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldFail_WhenEmailIsEmptyOrWhitespace(string email)
        {
            var result = Email.Create(email);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Email.Empty");
        }

        [Theory]
        [InlineData("john.doe.example.com")]   // no @
        [InlineData("john@example")]           // no dot after @
        [InlineData("john.doe@")]              // nothing after @
        public void Create_ShouldFail_WhenEmailFormatIsInvalid(string email)
        {
            var result = Email.Create(email);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Email.Invalid");
        }
    }
}
