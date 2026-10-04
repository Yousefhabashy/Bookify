using Bookify.Domain.Reviews;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories
{
    internal sealed class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Review review)
        {
            _context.Reviews.Add(review);
        }

        public async Task<bool> ExistsByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await _context.Reviews
                .AnyAsync(b => b.BookingId == bookingId, cancellationToken);
        }
    }
}
