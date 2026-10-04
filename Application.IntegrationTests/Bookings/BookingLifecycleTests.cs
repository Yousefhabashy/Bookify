using Application.IntegrationTests.Infrastructure;
using Bookify.Application.Abstractions;
using Bookify.Application.Bookings.CancelBooking;
using Bookify.Application.Bookings.ConfirmBooking;
using Bookify.Application.Bookings.ReserveBooking;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using Bookify.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Bookings
{
    public class BookingLifecycleTests : BaseIntegrationTest
    {
        public BookingLifecycleTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Reserve_ShouldFailValidationAndSaveNothing_WhenTheStayStartsInThePast()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();

            var result = await Sender.Send(
                new ReserveBookingCommand(apartmentId, PeriodFromToday(-5, 3)));

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ValidationError>();

            var count = await DbContext.Bookings.CountAsync(b => b.ApartmentId == apartmentId);
            count.Should().Be(0);
        }

        [Fact]
        public async Task Reserve_ShouldRejectABookingThatStartsOnTheCheckOutDay_ButAcceptTheNextDay()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            await ReserveAsync(apartmentId, PeriodFromToday(30, 3));

            var sameDay = await Sender.Send(
                new ReserveBookingCommand(apartmentId, PeriodFromToday(33, 3)));
            var nextDay = await Sender.Send(
                new ReserveBookingCommand(apartmentId, PeriodFromToday(34, 3)));

            sameDay.IsFailure.Should().BeTrue();
            sameDay.Error.Should().Be(BookingErrors.Overlap);
            nextDay.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Cancel_ShouldFreeTheDates_SoTheSamePeriodCanBeReservedAgain()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();

            var firstId = await ReserveAsync(apartmentId, PeriodFromToday(30, 3));
            var cancel = await Sender.Send(new CancelBookingCommand(firstId));
            var secondId = await ReserveAsync(apartmentId, PeriodFromToday(30, 3));

            cancel.IsSuccess.Should().BeTrue();
            secondId.Should().NotBe(firstId);

            var statuses = await DbContext.Bookings
                .AsNoTracking()
                .Where(b => b.ApartmentId == apartmentId)
                .Select(b => b.Status)
                .ToListAsync();

            statuses.Should().BeEquivalentTo(new[] { BookingStatus.Cancelled, BookingStatus.Reserved });
        }

        [Fact]
        public async Task Cancel_ShouldReturnNotFound_WhenAnotherUserTriesToCancel()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            var bookingId = await ReserveAsync(apartmentId, PeriodFromToday(30, 3));

            UserContext.UserId = Guid.NewGuid();

            var result = await Sender.Send(new CancelBookingCommand(bookingId));

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotFound);

            var saved = await DbContext.Bookings.AsNoTracking().SingleAsync(b => b.Id == bookingId);
            saved.Status.Should().Be(BookingStatus.Reserved);
        }

        [Fact]
        public async Task Confirm_ShouldBeForbiddenForNormalUsers_AndWorkForAdmins()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            var bookingId = await ReserveAsync(apartmentId, PeriodFromToday(30, 3));

            UserContext.IsAdmin = false;
            var denied = await Sender.Send(new ConfirmBookingCommand(bookingId));

            denied.IsFailure.Should().BeTrue();
            denied.Error.Type.Should().Be(ErrorType.Forbidden);

            UserContext.IsAdmin = true;
            var confirmed = await Sender.Send(new ConfirmBookingCommand(bookingId));

            confirmed.IsSuccess.Should().BeTrue();

            var saved = await DbContext.Bookings.AsNoTracking().SingleAsync(b => b.Id == bookingId);
            saved.Status.Should().Be(BookingStatus.Confirmed);
        }
    }
}