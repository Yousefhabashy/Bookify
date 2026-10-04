using Application.IntegrationTests.Infrastructure;
using Bookify.Application.Abstractions.Exceptions;
using Bookify.Infrastructure.Data;
using Docker.DotNet.Models;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests.Users
{
    public class UserSyncTests : BaseIntegrationTest
    {
        public UserSyncTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        private void SignIn()
        {
            UserContext.IsAuthenticated = true;
            UserContext.Email = $"{UserContext.UserId:N}@bookify.com";
        }

        private Task<bool> UserExistsAsync() =>
            DbContext.Users.AsNoTracking().AnyAsync(u => u.Id == UserContext.UserId);

        [Fact]
        public async Task Command_ShouldCreateTheLocalUser_WhenAnAuthenticatedUserIsNew()
        {
            SignIn();
            (await UserExistsAsync()).Should().BeFalse();

            await CreateApartmentAsync(); 

            (await UserExistsAsync()).Should().BeTrue();
        }

        [Fact]
        public async Task Command_ShouldNotCreateAUser_WhenTheRequestIsNotAuthenticated()
        {
            UserContext.IsAuthenticated = false;

            await CreateApartmentAsync();

            (await UserExistsAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task Command_ShouldSkipTheDatabaseCheck_WhenTheUserWasSyncedRecently()
        {
            SignIn();
            await CreateApartmentAsync();

            await DbContext.Users.Where(u => u.Id == UserContext.UserId).ExecuteDeleteAsync();
            DbContext.ChangeTracker.Clear();

            await CreateApartmentAsync();

            (await UserExistsAsync()).Should().BeFalse(); 
        }

        [Fact]
        public async Task Command_ShouldThrow_WhenTheIdentityProfileIsIncomplete()
        {
            SignIn();
            UserContext.Email = "";

            await Assert.ThrowsAsync<UserProfileIncompleteException>(() => CreateApartmentAsync());

            (await UserExistsAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task Command_ShouldCreateExactlyOneUser_WhenTwoFirstRequestsRace()
        {
            SignIn();

            async Task Attempt()
            {
                using var scope = Factory.Services.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await sender.Send(NewApartmentCommand());
                result.IsSuccess.Should().BeTrue();
            }

            await Task.WhenAll(Attempt(), Attempt());

            var count = await DbContext.Users.AsNoTracking().CountAsync(u => u.Id == UserContext.UserId);
            count.Should().Be(1);
        }
    }
}