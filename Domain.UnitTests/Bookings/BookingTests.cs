using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Bookings.Events;
using Bookify.Domain.Shared;
using Domain.UnitTests.Apartments;
using Domain.UnitTests.Infrastructure;
using FluentAssertions;

namespace Domain.UnitTests.Bookings
{
    public class BookingTests : BaseTest
    {
        // ───────────── Reserve ─────────────

        [Fact]
        public void Reserve_ShouldCreateAReservedBooking_WithTheCalculatedPrice()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(10m, Currency.Usd),
                cleaningFee: new Money(5m, Currency.Usd));
            var period = BookingData.ValidPeriod();

            var result = Booking.Reserve(
                apartment, BookingData.UserId, period, TestDates.Now, new PricingService());

            result.IsSuccess.Should().BeTrue();
            var booking = result.Value;
            booking.Status.Should().Be(BookingStatus.Reserved);
            booking.ApartmentId.Should().Be(apartment.Id);
            booking.UserId.Should().Be(BookingData.UserId);
            booking.Duration.Should().Be(period);
            booking.CreatedOnUtc.Should().Be(TestDates.Now);
            booking.PriceForPeriod.Should().Be(new Money(30m, Currency.Usd));
            booking.CleaningFee.Should().Be(new Money(5m, Currency.Usd));
            booking.TotalPrice.Should().Be(new Money(35m, Currency.Usd));
        }

        [Fact]
        public void Reserve_ShouldRaiseBookingReservedDomainEvent()
        {
            var booking = BookingData.CreateReserved();

            var domainEvent = AssertDomainEventWasPublished<BookingReservedDomainEvent>(booking);
            domainEvent.BookingId.Should().Be(booking.Id);
        }

        [Fact]
        public void Reserve_ShouldUpdateTheApartmentLastBookedDate()
        {
            var apartment = ApartmentData.CreateApartment();
            apartment.LastBookedOnUtc.Should().BeNull();

            BookingData.CreateReserved(apartment);

            apartment.LastBookedOnUtc.Should().Be(TestDates.Now);
        }

        [Fact]
        public void Reserve_ShouldFail_WhenApartmentCurrenciesDiffer()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(100m, Currency.Usd),
                cleaningFee: new Money(10m, Currency.Eur));

            var result = Booking.Reserve(
                apartment, BookingData.UserId, BookingData.ValidPeriod(), TestDates.Now, new PricingService());

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(MoneyErrors.CurrencyMismatch);
        }

        [Fact]
        public void Reserve_ShouldHaveNoSideEffects_WhenPricingFails()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(100m, Currency.Usd),
                cleaningFee: new Money(10m, Currency.Eur));

            Booking.Reserve(
                apartment, BookingData.UserId, BookingData.ValidPeriod(), TestDates.Now, new PricingService());

            apartment.LastBookedOnUtc.Should().BeNull();
        }

        // Depends on the rule "a stay cannot start in the past" being enforced by the domain.
        [Fact]
        public void Reserve_ShouldFail_WhenTheStayStartsInThePast()
        {
            var period = DateRange.Create(new DateOnly(2025, 12, 20), new DateOnly(2025, 12, 23)).Value;

            var result = Booking.Reserve(
                ApartmentData.CreateApartment(), BookingData.UserId, period, TestDates.Now, new PricingService());

            result.IsFailure.Should().BeTrue();
        }

        // ───────────── Confirm ─────────────

        [Fact]
        public void Confirm_ShouldConfirmAReservedBooking()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Reserved);
            var confirmedAt = TestDates.Now.AddHours(2);

            var result = booking.Confirm(confirmedAt);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Confirmed);
            booking.ConfirmedOnUtc.Should().Be(confirmedAt);
        }

        [Fact]
        public void Confirm_ShouldRaiseBookingConfirmedDomainEvent()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Reserved);

            booking.Confirm(TestDates.Now);

            var domainEvent = AssertDomainEventWasPublished<BookingConfirmedDomainEvent>(booking);
            domainEvent.BookingId.Should().Be(booking.Id);
        }

        [Theory]
        [InlineData(BookingStatus.Confirmed)]
        [InlineData(BookingStatus.Rejected)]
        [InlineData(BookingStatus.Cancelled)]
        [InlineData(BookingStatus.Completed)]
        public void Confirm_ShouldFail_WhenBookingIsNotReserved(BookingStatus status)
        {
            var booking = BookingData.CreateWithStatus(status);

            var result = booking.Confirm(TestDates.Now);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotReserved);
            booking.Status.Should().Be(status);
            AssertNoDomainEvents(booking);
        }

        // ───────────── Reject ─────────────

        [Fact]
        public void Reject_ShouldRejectAReservedBooking()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Reserved);
            var rejectedAt = TestDates.Now.AddMinutes(20);

            var result = booking.Reject(rejectedAt);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Rejected);
            booking.RejectedOnUtc.Should().Be(rejectedAt);
        }

        [Fact]
        public void Reject_ShouldRaiseBookingRejectedDomainEvent()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Reserved);

            booking.Reject(TestDates.Now);

            var domainEvent = AssertDomainEventWasPublished<BookingRejectedDomainEvent>(booking);
            domainEvent.BookingId.Should().Be(booking.Id);
        }

        [Theory]
        [InlineData(BookingStatus.Confirmed)]
        [InlineData(BookingStatus.Rejected)]
        [InlineData(BookingStatus.Cancelled)]
        [InlineData(BookingStatus.Completed)]
        public void Reject_ShouldFail_WhenBookingIsNotReserved(BookingStatus status)
        {
            var booking = BookingData.CreateWithStatus(status);

            var result = booking.Reject(TestDates.Now);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotReserved);
            booking.Status.Should().Be(status);
            AssertNoDomainEvents(booking);
        }

        // ───────────── Cancel ─────────────

        [Theory]
        [InlineData(BookingStatus.Reserved)]
        [InlineData(BookingStatus.Confirmed)]
        public void Cancel_ShouldCancelBeforeTheStayStarts(BookingStatus status)
        {
            var booking = BookingData.CreateWithStatus(status);
            var cancelledAt = TestDates.Now.AddDays(1);

            var result = booking.Cancel(cancelledAt);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Cancelled);
            booking.CancelledOnUtc.Should().Be(cancelledAt);
        }

        [Fact]
        public void Cancel_ShouldRaiseBookingCancelledDomainEvent()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Confirmed);

            booking.Cancel(TestDates.Now);

            var domainEvent = AssertDomainEventWasPublished<BookingCancelledDomainEvent>(booking);
            domainEvent.BookingId.Should().Be(booking.Id);
        }

        [Fact]
        public void Cancel_ShouldSucceed_OnTheStartDateItself()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Confirmed);

            var result = booking.Cancel(TestDates.AtEndOfDay(TestDates.CheckIn));

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Cancelled);
        }

        [Theory]
        [InlineData(BookingStatus.Reserved)]
        [InlineData(BookingStatus.Confirmed)]
        public void Cancel_ShouldFail_AfterTheStayHasStarted(BookingStatus status)
        {
            var booking = BookingData.CreateWithStatus(status);
            var dayAfterStart = TestDates.AtStartOfDay(TestDates.CheckIn.AddDays(1));

            var result = booking.Cancel(dayAfterStart);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.AlreadyStarted);
            booking.Status.Should().Be(status);
            AssertNoDomainEvents(booking);
        }

        [Theory]
        [InlineData(BookingStatus.Rejected)]
        [InlineData(BookingStatus.Cancelled)]
        [InlineData(BookingStatus.Completed)]
        public void Cancel_ShouldFail_WhenBookingIsNotReservedOrConfirmed(BookingStatus status)
        {
            var booking = BookingData.CreateWithStatus(status);

            var result = booking.Cancel(TestDates.Now);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotCancellable);
            booking.Status.Should().Be(status);
            AssertNoDomainEvents(booking);
        }

        // ───────────── Complete ─────────────

        [Fact]
        public void Complete_ShouldCompleteAConfirmedBooking_OnTheEndDate()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Confirmed);
            var completedAt = TestDates.AtStartOfDay(TestDates.CheckOut);

            var result = booking.Complete(completedAt);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Completed);
            booking.CompletedOnUtc.Should().Be(completedAt);
        }

        [Fact]
        public void Complete_ShouldSucceed_AfterTheEndDate()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Confirmed);

            var result = booking.Complete(TestDates.AtStartOfDay(TestDates.CheckOut.AddDays(5)));

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Complete_ShouldRaiseBookingCompletedDomainEvent()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Confirmed);

            booking.Complete(TestDates.AtStartOfDay(TestDates.CheckOut));

            var domainEvent = AssertDomainEventWasPublished<BookingCompletedDomainEvent>(booking);
            domainEvent.BookingId.Should().Be(booking.Id);
        }

        [Fact]
        public void Complete_ShouldFail_BeforeTheEndDate_AndLeaveTheBookingUntouched()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Confirmed);

            var result = booking.Complete(TestDates.AtEndOfDay(TestDates.CheckOut.AddDays(-1)));

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotEnded);
            booking.Status.Should().Be(BookingStatus.Confirmed);
            booking.CompletedOnUtc.Should().BeNull();
            AssertNoDomainEvents(booking);
        }

        [Theory]
        [InlineData(BookingStatus.Reserved)]
        [InlineData(BookingStatus.Rejected)]
        [InlineData(BookingStatus.Cancelled)]
        [InlineData(BookingStatus.Completed)]
        public void Complete_ShouldFail_WhenBookingIsNotConfirmed(BookingStatus status)
        {
            var booking = BookingData.CreateWithStatus(status);

            var result = booking.Complete(TestDates.AtStartOfDay(TestDates.CheckOut.AddDays(1)));

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotConfirmed);
            booking.Status.Should().Be(status);
            AssertNoDomainEvents(booking);
        }
    }
}
