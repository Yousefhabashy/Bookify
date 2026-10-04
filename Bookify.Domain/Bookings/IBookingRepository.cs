using Bookify.Domain.Apartments;

namespace Bookify.Domain.Bookings
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> IsOverlappingAsync(
            Apartment apartment,
            DateRange duration,
            CancellationToken cancellationToken = default
            );

        void Add(Booking booking);

        Task<IReadOnlyList<Guid>> RejectPendingBookingsJobAsync(
            DateTime threshold,
            CancellationToken cancellationToken = default
            );

        Task<IReadOnlyList<Guid>> GetCompletableBookingsAsync(
            DateOnly today,
            CancellationToken cancellationToken
            );

    }
}
