using Application.IntegrationTests.Infrastructure;
using Bookify.Application.Bookings.ReserveBooking;
using Bookify.Domain.Bookings;
using Bookify.Infrastructure.Data;
using Docker.DotNet.Models;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Reflection;

namespace Application.IntegrationTests.Bookings
{
    public class ReserveBookingTests : BaseIntegrationTest
    {
        private static readonly DateOnly Start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30); 
        private static DateRange Period() => DateRange.Create(Start, Start.AddDays(3)).Value;
        public ReserveBookingTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Database_ShouldRejectOverlappingBookings_EvenIfTheApplicationCheckIsBypassed()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            var apartment = await DbContext.Apartments.SingleAsync(a => a.Id == apartmentId);

            var pricing = new PricingService();

            var first = Booking.Reserve(apartment, UserContext.UserId, Period(), DateTime.UtcNow, pricing).Value;
            DbContext.Bookings.Add(first);
            await DbContext.SaveChangesAsync();

            var second = Booking.Reserve(apartment, UserContext.UserId, Period(), DateTime.UtcNow, pricing).Value;
            DbContext.Bookings.Add(second);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => DbContext.SaveChangesAsync());

            exception.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().Be(PostgresErrorCodes.ExclusionViolation);
        }

        [Fact]
        public async Task Reserve_ShouldReturnOverlapError_WhenDatesAreAlreadyBooked()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();

            var first = await Sender.Send(new ReserveBookingCommand(apartmentId, Period()));
            var second = await Sender.Send(new ReserveBookingCommand(apartmentId, Period()));

            first.IsSuccess.Should().BeTrue();
            second.IsFailure.Should().BeTrue();
            second.Error.Should().Be(BookingErrors.Overlap);
        }

        [Fact]
        public async Task Reserve_ShouldLetOnlyOneRequestWin_WhenTwoRequestsRaceForTheSameDates()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();

            async Task<(Bookify.Domain.Abstractions.Result<Guid>? Result, Exception? Error)> Attempt()
            {
                using var scope = Factory.Services.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                try
                {
                    return (await sender.Send(new ReserveBookingCommand(apartmentId, Period())), null);
                }
                catch (Exception ex)
                {
                    return (null, ex);
                }
            }

            var attempts = await Task.WhenAll(Attempt(), Attempt());

            attempts.Count(a => a.Result is { IsSuccess: true }).Should().Be(1);

            var loser = attempts.Single(a => a.Result is not { IsSuccess: true });
            bool lostOnCheck = loser.Result?.Error == BookingErrors.Overlap;
            bool lostOnConstraint = loser.Error is DbUpdateException
            {
                InnerException: PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation }
            };
            (lostOnCheck || lostOnConstraint).Should().BeTrue();

            var bookingsCount = await DbContext.Bookings
                .AsNoTracking()
                .CountAsync(b => b.ApartmentId == apartmentId);
            bookingsCount.Should().Be(1);
        }
    }
}