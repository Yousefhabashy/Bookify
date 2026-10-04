using Application.IntegrationTests.Context;
using Bookify.Application.Apartments.CreateApartment;
using Bookify.Application.Bookings.ReserveBooking;
using Bookify.Domain.Bookings;
using Bookify.Domain.Users;
using Bookify.Infrastructure.Data;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.IntegrationTests.Infrastructure
{
    [CollectionDefinition("Integration")]
    public class IntegrationCollection : ICollectionFixture<IntegrationTestWebAppFactory> { }

    [Collection("Integration")]
    public abstract class BaseIntegrationTest : IDisposable
    {
        protected IntegrationTestWebAppFactory Factory { get; }
        private readonly IServiceScope _scope;
        protected ISender Sender { get; }
        protected ApplicationDbContext DbContext { get; }
        protected TestUserContext UserContext { get; }

        protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
        {
            Factory = factory;
            _scope = factory.Services.CreateScope();
            Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
            DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            UserContext = factory.UserContext;
            UserContext.IsAuthenticated = false;
            UserContext.IsAdmin = false;
            UserContext.UserId = Guid.NewGuid();
        }
        protected async Task CreateUserAsync()
        {
            var user = User.Create(
                UserContext.UserId,
                FirstName.Create("Test").Value,
                LastName.Create("User").Value,
                Email.Create($"{UserContext.UserId:N}@bookify.com").Value).Value;

            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();
        }
        protected static CreateApartmentCommand NewApartmentCommand() => new(
            Name: "Test Apartment",
            Description: "A nice place to stay.",
            Country: "Egypt", State: "Cairo", City: "Cairo", Street: "Tahrir St", ZipCode: "11511",
            PriceAmount: 100m, PriceCurrency: "USD",
            CleaningFeeAmount: 10m, CleaningFeeCurrency: "USD",
            Amenities: new List<int> { 1 }
            );

        protected async Task<Guid> CreateApartmentAsync()
        {
            var result = await Sender.Send(NewApartmentCommand());
            result.IsSuccess.Should().BeTrue();
            return result.Value;
        }

        protected static DateOnly InDays(int days) =>
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

        protected static DateRange PeriodFromToday(int startInDays, int nights) =>
            DateRange.Create(InDays(startInDays), InDays(startInDays + nights)).Value;

        protected async Task<Guid> ReserveAsync(Guid apartmentId, DateRange period)
        {
            var result = await Sender.Send(new ReserveBookingCommand(apartmentId, period));
            result.IsSuccess.Should().BeTrue();
            return result.Value;
        }

        protected async Task<Guid> CreateCompletedBookingAsync(Guid apartmentId)
        {
            var period = PeriodFromToday(30, 3);
            var bookingId = await ReserveAsync(apartmentId, period);

            var booking = await DbContext.Bookings.SingleAsync(b => b.Id == bookingId);
            booking.Confirm(DateTime.UtcNow).IsSuccess.Should().BeTrue();
            booking.Complete(period.End.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
                .IsSuccess.Should().BeTrue();
            await DbContext.SaveChangesAsync();

            return bookingId;
        }

        protected async Task ClearOutboxAsync()
        {
            await DbContext.OutboxMessages.ExecuteDeleteAsync();
            DbContext.ChangeTracker.Clear();
        }
        public void Dispose() => _scope.Dispose();
    }
}
