using Application.IntegrationTests.Infrastructure;
using Bookify.Application.Reviews.CreateReview;
using Bookify.Domain.Reviews;
using Bookify.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.IntegrationTests.Reviews
{
    public class CreateReviewTests : BaseIntegrationTest
    {
        public CreateReviewTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateReview_ShouldPersistTheReview_WhenBookingIsCompleted()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            var bookingId = await CreateCompletedBookingAsync(apartmentId);

            var result = await Sender.Send(new CreateReviewCommand(bookingId, 4, "Clean and quiet."));

            result.IsSuccess.Should().BeTrue();

            var saved = await DbContext.Reviews.AsNoTracking().SingleAsync(r => r.Id == result.Value);
            saved.BookingId.Should().Be(bookingId);
            saved.ApartmentId.Should().Be(apartmentId);
            saved.UserId.Should().Be(UserContext.UserId);
            saved.Rating.Value.Should().Be(4);
            saved.Comment.Value.Should().Be("Clean and quiet.");
        }

        [Fact]
        public async Task CreateReview_ShouldFail_WhenTheBookingIsNotCompleted()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            var bookingId = await ReserveAsync(apartmentId, PeriodFromToday(30, 3));

            var result = await Sender.Send(new CreateReviewCommand(bookingId, 5, "Great"));

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ReviewErrors.NotEligible);
        }

        [Fact]
        public async Task CreateReview_ShouldFailWithAlreadyExists_WhenTheBookingIsReviewedTwice()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            var bookingId = await CreateCompletedBookingAsync(apartmentId);

            var first = await Sender.Send(new CreateReviewCommand(bookingId, 5, "Great"));
            var second = await Sender.Send(new CreateReviewCommand(bookingId, 1, "Changed my mind"));

            first.IsSuccess.Should().BeTrue();
            second.IsFailure.Should().BeTrue();
            second.Error.Should().Be(ReviewErrors.AlreadyExists);
        }

        [Fact]
        public async Task Database_ShouldRejectTwoReviewsForTheSameBooking_EvenIfTheApplicationCheckIsBypassed()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            var bookingId = await CreateCompletedBookingAsync(apartmentId);
            var booking = await DbContext.Bookings.SingleAsync(b => b.Id == bookingId);

            var first = Review.Create(booking, Rating.Create(5).Value, Comment.Create("First").Value, DateTime.UtcNow).Value;
            var second = Review.Create(booking, Rating.Create(4).Value, Comment.Create("Second").Value, DateTime.UtcNow).Value;
            DbContext.Reviews.Add(first);
            DbContext.Reviews.Add(second);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => DbContext.SaveChangesAsync());

            exception.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        }
    }
}