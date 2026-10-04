using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories
{
    internal sealed class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Add(Booking booking)
        {
            _context.Bookings.Add(booking);
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<bool> IsOverlappingAsync(Apartment apartment, DateRange duration, CancellationToken cancellationToken = default)
        {
            var activeBookingStatuses = new[]
            {
                BookingStatus.Reserved,
                BookingStatus.Confirmed,
                BookingStatus.Completed
            };

            return await _context.Bookings
                .AnyAsync(booking =>
                    booking.ApartmentId == apartment.Id &&
                    booking.Duration.Start <= duration.End &&
                    booking.Duration.End >= duration.Start &&
                    activeBookingStatuses.Contains(booking.Status),
                    cancellationToken
                    );
        }

        public async Task<IReadOnlyList<Guid>> RejectPendingBookingsJobAsync(
            DateTime threshold,
            CancellationToken cancellationToken = default
            )
        {
            return await _context.Bookings
                .Where(b => b.Status == BookingStatus.Reserved && b.CreatedOnUtc <= threshold)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetCompletableBookingsAsync(
            DateOnly today,
            CancellationToken cancellationToken
            )
        {
            return await _context.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed && b.Duration.End <= today)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
