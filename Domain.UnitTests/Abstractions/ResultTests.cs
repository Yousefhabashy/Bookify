using Bookify.Domain.Abstractions;
using FluentAssertions;

namespace Domain.UnitTests.Abstractions
{
    public class ResultTests
    {
        private static readonly Error SomeError = new("Some.Error", "Something went wrong", ErrorType.Conflict);

        [Fact]
        public void Success_ShouldHaveNoError()
        {
            var result = Result.Success();

            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Error.Should().Be(Error.None);
        }

        [Fact]
        public void Failure_ShouldCarryTheError()
        {
            var result = Result.Failure(SomeError);

            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(SomeError);
        }

        [Fact]
        public void Failure_ShouldThrow_WhenErrorIsNone()
        {
            var act = () => Result.Failure(Error.None);

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GenericSuccess_ShouldExposeValue()
        {
            var result = Result.Success(42);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
        }

        [Fact]
        public void GenericFailure_ShouldThrow_WhenValueIsAccessed()
        {
            var result = Result.Failure<int>(SomeError);

            var act = () => result.Value;

            result.IsFailure.Should().BeTrue();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenValueIsNotNull()
        {
            var result = Result.Create("hello");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("hello");
        }

        [Fact]
        public void Create_ShouldReturnNullValueError_WhenValueIsNull()
        {
            var result = Result.Create<string>(null);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(Error.NullValue);
        }

        [Fact]
        public void ImplicitConversion_ShouldWrapValueInSuccessResult()
        {
            Result<string> result = "value";

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("value");
        }
    }
}
