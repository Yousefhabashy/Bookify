using Application.UnitTests.Common;
using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Reviews.CreateReview;
using Bookify.Domain.Bookings;
using Bookify.Domain.Reviews;
using FluentAssertions;
using NSubstitute;

namespace Application.UnitTests.Reviews
{
    public class CreateReviewCommandHandlerTests
    {
        private static readonly Guid OwnerId = Guid.NewGuid();
        private static readonly DateTime ReviewTime = TestData.AtStartOfDay(TestData.CheckOut.AddDays(1));

        private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
        private readonly IReviewRepository _reviewRepository = Substitute.For<IReviewRepository>();
        private readonly IUserContext _userContext = Substitute.For<IUserContext>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly CreateReviewCommandHandler _handler;

        public CreateReviewCommandHandlerTests()
        {
            _userContext.UserId.Returns(OwnerId);
            _dateTimeProvider.UtcNow.Returns(ReviewTime);

            _handler = new CreateReviewCommandHandler(
                _bookingRepository,
                _reviewRepository,
                _userContext,
                _unitOfWork,
                _dateTimeProvider);
        }

        private void ArrangeBooking(Booking booking) =>
            _bookingRepository
                .GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
                .Returns(booking);

        private async Task AssertNothingWasSaved()
        {
            _reviewRepository.DidNotReceive().Add(Arg.Any<Review>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenBookingIsNotFound()
        {
            var bookingId = Guid.NewGuid();
            _bookingRepository
                .GetByIdAsync(bookingId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Booking?>(null));

            var result = await _handler.Handle(
                new CreateReviewCommand(bookingId, 5, "Great"), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotFound);
            await AssertNothingWasSaved();
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenBookingBelongsToAnotherUser()
        {
            var booking = TestData.CreateBooking(BookingStatus.Completed, userId: Guid.NewGuid());
            ArrangeBooking(booking);

            var result = await _handler.Handle(
                new CreateReviewCommand(booking.Id, 5, "Great"), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.NotFound);
            await AssertNothingWasSaved();
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenTheBookingWasAlreadyReviewed()
        {
            var booking = TestData.CreateBooking(BookingStatus.Completed, OwnerId);
            ArrangeBooking(booking);
            _reviewRepository
                .ExistsByBookingIdAsync(booking.Id, Arg.Any<CancellationToken>())
                .Returns(true);

            var result = await _handler.Handle(
                new CreateReviewCommand(booking.Id, 5, "Great"), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ReviewErrors.AlreadyExists);
            await AssertNothingWasSaved();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async Task Handle_ShouldFail_WhenRatingIsOutOfRange(int rating)
        {
            var booking = TestData.CreateBooking(BookingStatus.Completed, OwnerId);
            ArrangeBooking(booking);

            var result = await _handler.Handle(
                new CreateReviewCommand(booking.Id, rating, "Great"), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(Rating.Invalid);
            await AssertNothingWasSaved();
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenCommentIsEmpty()
        {
            var booking = TestData.CreateBooking(BookingStatus.Completed, OwnerId);
            ArrangeBooking(booking);

            var result = await _handler.Handle(
                new CreateReviewCommand(booking.Id, 5, "   "), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Comment.Empty");
            await AssertNothingWasSaved();
        }

        [Theory]
        [InlineData(BookingStatus.Reserved)]
        [InlineData(BookingStatus.Confirmed)]
        public async Task Handle_ShouldFail_WhenTheBookingIsNotCompleted(BookingStatus status)
        {
            var booking = TestData.CreateBooking(status, OwnerId);
            ArrangeBooking(booking);

            var result = await _handler.Handle(
                new CreateReviewCommand(booking.Id, 5, "Great"), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ReviewErrors.NotEligible);
            await AssertNothingWasSaved();
        }

        [Fact]
        public async Task Handle_ShouldAddTheReviewAndSave_WhenEverythingIsValid()
        {
            var booking = TestData.CreateBooking(BookingStatus.Completed, OwnerId);
            ArrangeBooking(booking);

            var result = await _handler.Handle(
                new CreateReviewCommand(booking.Id, 4, "Clean and quiet."), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();

            _reviewRepository.Received(1).Add(Arg.Is<Review>(r =>
                r.Id == result.Value &&
                r.BookingId == booking.Id &&
                r.ApartmentId == booking.ApartmentId &&
                r.UserId == OwnerId &&
                r.Rating.Value == 4 &&
                r.Comment.Value == "Clean and quiet." &&
                r.CreatedOnUtc == ReviewTime));

            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}