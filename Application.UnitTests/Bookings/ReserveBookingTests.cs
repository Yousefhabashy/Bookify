using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.ReserveBooking;
using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Shared;
using FluentAssertions;
using NSubstitute;

namespace Application.UnitTests.Bookings
{
    public class ReserveBookingCommandHandlerTests
    {
        private static readonly DateTime Now = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly DateRange FuturePeriod =
            DateRange.Create(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 13)).Value;

        private static readonly DateRange PastPeriod =
            DateRange.Create(new DateOnly(2025, 12, 20), new DateOnly(2025, 12, 23)).Value;

        private readonly IApartmentRepository _apartmentRepository = Substitute.For<IApartmentRepository>();
        private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly IUserContext _userContext = Substitute.For<IUserContext>();
        private readonly ReserveBookingCommandHandler _handler;

        public ReserveBookingCommandHandlerTests()
        {
            _dateTimeProvider.UtcNow.Returns(Now);
            _userContext.UserId.Returns(UserId);

            _handler = new ReserveBookingCommandHandler(
                _apartmentRepository,
                _bookingRepository,
                _unitOfWork,
                _dateTimeProvider,
                new PricingService(),
                _userContext);
        }

        private static Apartment CreateApartment() =>
            Apartment.Create(
                Guid.NewGuid(),
                Name.Create("Test Apartment").Value,
                Description.Create("A nice place to stay.").Value,
                Address.Create("Egypt", "Cairo", "11511", "Cairo", "Tahrir St").Value,
                new Money(100m, Currency.Usd),
                Money.Zero(Currency.Usd),
                new List<Amenity> { Amenity.WiFi }).Value;

        private Apartment ArrangeExistingApartment()
        {
            var apartment = CreateApartment();
            _apartmentRepository
                .GetByIdAsync(apartment.Id, Arg.Any<CancellationToken>())
                .Returns(apartment);
            return apartment;
        }

        private async Task AssertNothingWasSaved()
        {
            _bookingRepository.DidNotReceive().Add(Arg.Any<Booking>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenApartmentIsNotFound()
        {
            var command = new ReserveBookingCommand(Guid.NewGuid(), FuturePeriod);
            _apartmentRepository
                .GetByIdAsync(command.ApartmentId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Apartment?>(null));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApartmentErrors.NotFound);
            await AssertNothingWasSaved();
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenDatesOverlapAnotherBooking()
        {
            var apartment = ArrangeExistingApartment();
            _bookingRepository
                .IsOverlappingAsync(apartment, FuturePeriod, Arg.Any<CancellationToken>())
                .Returns(true);

            var result = await _handler.Handle(
                new ReserveBookingCommand(apartment.Id, FuturePeriod), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.Overlap);
            await AssertNothingWasSaved();
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenTheStayStartsInThePast()
        {
            var apartment = ArrangeExistingApartment();
            _bookingRepository
                .IsOverlappingAsync(apartment, PastPeriod, Arg.Any<CancellationToken>())
                .Returns(false);

            var result = await _handler.Handle(
                new ReserveBookingCommand(apartment.Id, PastPeriod), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.StartDateInThePast);
            await AssertNothingWasSaved();
        }

        [Fact]
        public async Task Handle_ShouldReserveAndSave_WhenEverythingIsValid()
        {
            var apartment = ArrangeExistingApartment();
            _bookingRepository
                .IsOverlappingAsync(apartment, FuturePeriod, Arg.Any<CancellationToken>())
                .Returns(false);

            var result = await _handler.Handle(
                new ReserveBookingCommand(apartment.Id, FuturePeriod), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();

            _bookingRepository.Received(1).Add(Arg.Is<Booking>(b =>
                b.Id == result.Value &&
                b.ApartmentId == apartment.Id &&
                b.UserId == UserId &&
                b.Status == BookingStatus.Reserved));

            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}