using Application.UnitTests.Common;
using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.ConfirmBooking;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using FluentAssertions;
using NSubstitute;

namespace Application.UnitTests.Bookings
{
    public class ConfirmBookingCommandHandlerTests
    {
        private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly IUserContext _userContext = Substitute.For<IUserContext>();
        private readonly ConfirmBookingCommandHandler _handler;

        public ConfirmBookingCommandHandlerTests()
        {
            _dateTimeProvider.UtcNow.Returns(TestData.Now);
            _userContext.IsAdmin.Returns(true);

            _handler = new ConfirmBookingCommandHandler(
                _bookingRepository, _unitOfWork, _dateTimeProvider, _userContext);
        }

        private void ArrangeBooking(Booking booking) =>
            _bookingRepository
                .GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
                .Returns(booking);

        [Fact]
        public async Task Handle_ShouldFailWithoutTouchingTheRepository_WhenUserIsNotAdmin()
        {
            _userContext.IsAdmin.Returns(false);

            var result = await _handler.Handle(new ConfirmBookingCommand(Guid.NewGuid()), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Forbidden);
            await _bookingRepository.DidNotReceive()
                .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenBookingIsNotFound()
        {
            var bookingId = Guid.NewGuid();
            _bookingRepository
                .GetByIdAsync(bookingId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Booking?>(null));

            var result = await _handler.Handle(new ConfirmBookingCommand(bookingId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldConfirmAndSave_WhenAdminConfirmsAReservedBooking()
        {
            var booking = TestData.CreateBooking(BookingStatus.Reserved);
            ArrangeBooking(booking);

            var result = await _handler.Handle(new ConfirmBookingCommand(booking.Id), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Confirmed);
            booking.ConfirmedOnUtc.Should().Be(TestData.Now);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(BookingStatus.Confirmed)]
        [InlineData(BookingStatus.Rejected)]
        [InlineData(BookingStatus.Cancelled)]
        [InlineData(BookingStatus.Completed)]
        public async Task Handle_ShouldFailAndNotSave_WhenBookingIsNotReserved(BookingStatus status)
        {
            var booking = TestData.CreateBooking(status);
            ArrangeBooking(booking);

            var result = await _handler.Handle(new ConfirmBookingCommand(booking.Id), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotReserved);
            booking.Status.Should().Be(status);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}