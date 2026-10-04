namespace Bookify.Domain.Reviews
{
    public interface IReviewRepository
    {
        Task<bool> ExistsByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
        void Add(Review review);
    }
}
