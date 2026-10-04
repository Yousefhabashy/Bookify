using Bookify.Domain.Bookings;
using Bookify.Domain.Reviews;
using Bookify.Domain.Reviews.Events;
using Domain.UnitTests.Bookings;
using Domain.UnitTests.Infrastructure;
using FluentAssertions;

namespace Domain.UnitTests.Reviews
{
    public class ReviewTests : BaseTest
    {
        private static readonly Rating ValidRating = Rating.Create(5).Value;
        private static readonly Comment ValidComment = Comment.Create("Loved it.").Value;

        [Fact]
        public void Create_ShouldSucceed_ForACompletedBooking()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Completed);
            var createdAt = TestDates.AtStartOfDay(TestDates.CheckOut.AddDays(1));

            var result = Review.Create(booking, ValidRating, ValidComment, createdAt);

            result.IsSuccess.Should().BeTrue();
            var review = result.Value;
            review.BookingId.Should().Be(booking.Id);
            review.ApartmentId.Should().Be(booking.ApartmentId);
            review.UserId.Should().Be(booking.UserId);
            review.Rating.Should().Be(ValidRating);
            review.Comment.Should().Be(ValidComment);
            review.CreatedOnUtc.Should().Be(createdAt);
        }

        [Fact]
        public void Create_ShouldRaiseReviewCreatedDomainEvent()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Completed);

            var result = Review.Create(booking, ValidRating, ValidComment, TestDates.Now);

            var domainEvent = AssertDomainEventWasPublished<ReviewCreatedDomainEvent>(result.Value);
            domainEvent.ReviewId.Should().Be(result.Value.Id);
        }

        [Theory]
        [InlineData(BookingStatus.Reserved)]
        [InlineData(BookingStatus.Confirmed)]
        [InlineData(BookingStatus.Rejected)]
        [InlineData(BookingStatus.Cancelled)]
        public void Create_ShouldFail_WhenBookingIsNotCompleted(BookingStatus status)
        {
            var booking = BookingData.CreateWithStatus(status);

            var result = Review.Create(booking, ValidRating, ValidComment, TestDates.Now);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ReviewErrors.NotEligible);
        }

        [Fact]
        public void Create_ShouldFail_WhenRatingIsNull()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Completed);

            var result = Review.Create(booking, null!, ValidComment, TestDates.Now);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Review.MissingRating");
        }

        [Fact]
        public void Create_ShouldFail_WhenCommentIsNull()
        {
            var booking = BookingData.CreateWithStatus(BookingStatus.Completed);

            var result = Review.Create(booking, ValidRating, null!, TestDates.Now);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Review.MissingComment");
        }
    }
}
