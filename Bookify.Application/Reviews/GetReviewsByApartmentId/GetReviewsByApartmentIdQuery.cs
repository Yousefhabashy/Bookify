using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Reviews.GetReviewsByApartmentId
{
    public sealed record GetReviewsByApartmentIdQuery(Guid ApartmentId) : IQuery<IReadOnlyCollection<ReviewResponse>>;
}
