using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Reviews.GetReviewById
{
    public sealed record GetReviewByIdQuery(Guid ReviewId) : IQuery<ReviewResponse>;
}
