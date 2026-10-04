using Bookify.Domain.Reviews;
using FluentAssertions;

namespace Domain.UnitTests.Reviews
{
    public class CommentTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenCommentIsValid()
        {
            var result = Comment.Create("Great place, very clean.");

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be("Great place, very clean.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldFail_WhenCommentIsEmptyOrWhitespace(string comment)
        {
            var result = Comment.Create(comment);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Comment.Empty");
        }

        [Fact]
        public void Create_ShouldAccept_ExactlyTheMaximumLength()
        {
            // The database column is varchar(400): keep both limits in sync.
            Comment.Create(new string('a', 400)).IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldFail_WhenCommentIsLongerThanTheMaximum()
        {
            var result = Comment.Create(new string('a', 401));

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Comment.TooLong");
        }
    }
}
