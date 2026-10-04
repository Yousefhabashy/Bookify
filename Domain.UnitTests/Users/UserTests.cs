using Bookify.Domain.Users;
using Bookify.Domain.Users.Events;
using Domain.UnitTests.Infrastructure;
using FluentAssertions;

namespace Domain.UnitTests.Users
{
    public class UserTests : BaseTest
    {
        [Fact]
        public void Create_ShouldSucceed_WhenValidDetailsAreProvided()
        {
            var userId = Guid.NewGuid();

            var result = User.Create(
                userId,
                UserData.ValidFirstName,
                UserData.ValidLastName,
                UserData.ValidEmail);

            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(userId);
            result.Value.FirstName.Should().Be(UserData.ValidFirstName);
            result.Value.LastName.Should().Be(UserData.ValidLastName);
            result.Value.Email.Should().Be(UserData.ValidEmail);
        }

        [Fact]
        public void Create_ShouldRaiseUserCreatedDomainEvent()
        {
            var userId = Guid.NewGuid();

            var result = User.Create(userId, UserData.ValidFirstName, UserData.ValidLastName, UserData.ValidEmail);

            var domainEvent = AssertDomainEventWasPublished<UserCreatedDomainEvent>(result.Value);
            domainEvent.UserId.Should().Be(userId);
        }

        [Fact]
        public void Create_ShouldFail_WhenFirstNameIsNull()
        {
            var result = User.Create(Guid.NewGuid(), null!, UserData.ValidLastName, UserData.ValidEmail);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("User.InvalidDetails");
        }

        [Fact]
        public void Create_ShouldFail_WhenLastNameIsNull()
        {
            var result = User.Create(Guid.NewGuid(), UserData.ValidFirstName, null!, UserData.ValidEmail);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("User.InvalidDetails");
        }

        [Fact]
        public void Create_ShouldFail_WhenEmailIsNull()
        {
            var result = User.Create(Guid.NewGuid(), UserData.ValidFirstName, UserData.ValidLastName, null!);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("User.InvalidDetails");
        }
    }
}
