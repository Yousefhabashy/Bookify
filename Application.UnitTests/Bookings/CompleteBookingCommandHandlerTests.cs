using Application.UnitTests.Common;
using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.CompleteBooking;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using FluentAssertions;
using NSubstitute;

namespace Application.UnitTests.Bookings
{
    public class CompleteBookingCommandHandlerTests
    {
        // A moment on the check-out day: the stay is over.
        private static readonly DateTime AfterTheStay = TestData.AtStartOfDay(TestData.CheckOut);

        private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly IUserContext _userContext = Substitute.For<IUserContext>();
        private readonly CompleteBookingCommandHandler _handler;

        public CompleteBookingCommandHandlerTests()
        {
            _dateTimeProvider.UtcNow.Returns(AfterTheStay);
            _userContext.IsAdmin.Returns(true);

            _handler = new CompleteBookingCommandHandler(
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

            var result = await _handler.Handle(new CompleteBookingCommand(Guid.NewGuid()), CancellationToken.None);

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

            var result = await _handler.Handle(new CompleteBookingCommand(bookingId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldCompleteAndSave_WhenAConfirmedBookingHasEnded()
        {
            var booking = TestData.CreateBooking(BookingStatus.Confirmed);
            ArrangeBooking(booking);

            var result = await _handler.Handle(new CompleteBookingCommand(booking.Id), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Completed);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFailAndNotSave_WhenTheStayHasNotEndedYet()
        {
            var booking = TestData.CreateBooking(BookingStatus.Confirmed);
            ArrangeBooking(booking);
            _dateTimeProvider.UtcNow.Returns(TestData.AtStartOfDay(TestData.CheckOut.AddDays(-1)));

            var result = await _handler.Handle(new CompleteBookingCommand(booking.Id), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotEnded);
            booking.Status.Should().Be(BookingStatus.Confirmed);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFailAndNotSave_WhenBookingIsNotConfirmed()
        {
            var booking = TestData.CreateBooking(BookingStatus.Reserved);
            ArrangeBooking(booking);

            var result = await _handler.Handle(new CompleteBookingCommand(booking.Id), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotConfirmed);
            booking.Status.Should().Be(BookingStatus.Reserved);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}