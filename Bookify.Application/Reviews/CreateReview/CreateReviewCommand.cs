using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Reviews.CreateReview
{
    public sealed record CreateReviewCommand(
        Guid BookingId,
        int Rating,
        string Comment
        ) : ICommand<Guid>;
}
