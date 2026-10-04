namespace Bookify.Application.Reviews.GetReviewById
{
    public sealed class ReviewResponse
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public int Rating { get; init; }
        public string Comment { get; init; }
        public DateTime CreatedOnUtc { get; init; }
    }
}
