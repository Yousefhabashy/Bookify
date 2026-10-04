using Application.UnitTests.Common;
using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.AutoCompleteBooking;
using Bookify.Application.Bookings.AutoRejectBooking;
using Bookify.Domain.Bookings;
using FluentAssertions;
using NSubstitute;

namespace Application.UnitTests.Bookings
{
    public class AutoRejectBookingHandlerTests
    {
        private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly AutoRejectBookingHandler _handler;

        public AutoRejectBookingHandlerTests()
        {
            _dateTimeProvider.UtcNow.Returns(TestData.Now.AddMinutes(20));
            _handler = new AutoRejectBookingHandler(_bookingRepository, _unitOfWork, _dateTimeProvider);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenBookingIsNotFound()
        {
            var bookingId = Guid.NewGuid();
            _bookingRepository
                .GetByIdAsync(bookingId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Booking?>(null));

            var result = await _handler.Handle(new AutoRejectBookingCommand(bookingId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldRejectAndSave_WhenBookingIsReserved()
        {
            var booking = TestData.CreateBooking(BookingStatus.Reserved);
            _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);

            var result = await _handler.Handle(new AutoRejectBookingCommand(booking.Id), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Rejected);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFailAndNotSave_WhenAdminAlreadyConfirmedTheBooking()
        {
            // The job may pick up a booking that was confirmed a moment earlier.
            var booking = TestData.CreateBooking(BookingStatus.Confirmed);
            _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);

            var result = await _handler.Handle(new AutoRejectBookingCommand(booking.Id), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotReserved);
            booking.Status.Should().Be(BookingStatus.Confirmed);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    public class AutoCompleteBookingHandlerTests
    {
        private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly AutoCompleteBookingHandler _handler;

        public AutoCompleteBookingHandlerTests()
        {
            _dateTimeProvider.UtcNow.Returns(TestData.AtStartOfDay(TestData.CheckOut));
            _handler = new AutoCompleteBookingHandler(_bookingRepository, _unitOfWork, _dateTimeProvider);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenBookingIsNotFound()
        {
            var bookingId = Guid.NewGuid();
            _bookingRepository
                .GetByIdAsync(bookingId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Booking?>(null));

            var result = await _handler.Handle(new AutoCompleteBookingCommand(bookingId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldCompleteAndSave_WhenConfirmedBookingHasEnded()
        {
            var booking = TestData.CreateBooking(BookingStatus.Confirmed);
            _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);

            var result = await _handler.Handle(new AutoCompleteBookingCommand(booking.Id), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Completed);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFailAndNotSave_WhenTheStayHasNotEnded()
        {
            var booking = TestData.CreateBooking(BookingStatus.Confirmed);
            _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
            _dateTimeProvider.UtcNow.Returns(TestData.AtStartOfDay(TestData.CheckOut.AddDays(-1)));

            var result = await _handler.Handle(new AutoCompleteBookingCommand(booking.Id), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotEnded);
            booking.Status.Should().Be(BookingStatus.Confirmed);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}