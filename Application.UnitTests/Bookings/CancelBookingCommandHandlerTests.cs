using Application.UnitTests.Common;
using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.CancelBooking;
using Bookify.Domain.Bookings;
using FluentAssertions;
using NSubstitute;

namespace Application.UnitTests.Bookings
{
    public class CancelBookingCommandHandlerTests
    {
        private static readonly Guid OwnerId = Guid.NewGuid();

        private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly IUserContext _userContext = Substitute.For<IUserContext>();
        private readonly CancelBookingCommandHandler _handler;

        public CancelBookingCommandHandlerTests()
        {
            _dateTimeProvider.UtcNow.Returns(TestData.Now);
            _userContext.UserId.Returns(OwnerId);

            _handler = new CancelBookingCommandHandler(
                _bookingRepository, _unitOfWork, _dateTimeProvider, _userContext);
        }

        private void ArrangeBooking(Booking booking) =>
            _bookingRepository
                .GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
                .Returns(booking);

        [Fact]
        public async Task Handle_ShouldFail_WhenBookingIsNotFound()
        {
            var bookingId = Guid.NewGuid();
            _bookingRepository
                .GetByIdAsync(bookingId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Booking?>(null));

            var result = await _handler.Handle(new CancelBookingCommand(bookingId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenBookingBelongsToAnotherUser()
        {
            // NotFound (not Forbidden): we never confirm that someone else's booking exists.
            var booking = TestData.CreateBooking(BookingStatus.Reserved, userId: Guid.NewGuid());
            ArrangeBooking(booking);

            var result = await _handler.Handle(new CancelBookingCommand(booking.Id), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotFound);
            booking.Status.Should().Be(BookingStatus.Reserved);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(BookingStatus.Reserved)]
        [InlineData(BookingStatus.Confirmed)]
        public async Task Handle_ShouldCancelAndSave_WhenOwnerCancelsBeforeTheStayStarts(BookingStatus status)
        {
            var booking = TestData.CreateBooking(status, OwnerId);
            ArrangeBooking(booking);

            var result = await _handler.Handle(new CancelBookingCommand(booking.Id), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Cancelled);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFailAndNotSave_WhenTheDomainRefusesToCancel()
        {
            var booking = TestData.CreateBooking(BookingStatus.Completed, OwnerId);
            ArrangeBooking(booking);

            var result = await _handler.Handle(new CancelBookingCommand(booking.Id), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotCancellable);
            booking.Status.Should().Be(BookingStatus.Completed);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFailAndNotSave_WhenTheStayHasAlreadyStarted()
        {
            var booking = TestData.CreateBooking(BookingStatus.Confirmed, OwnerId);
            ArrangeBooking(booking);
            _dateTimeProvider.UtcNow.Returns(TestData.AtStartOfDay(TestData.CheckIn.AddDays(1)));

            var result = await _handler.Handle(new CancelBookingCommand(booking.Id), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.AlreadyStarted);
            booking.Status.Should().Be(BookingStatus.Confirmed);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}