using Bookify.Domain.Apartments;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories
{
    internal sealed class ApartmentRepository : IApartmentRepository
    {
        private readonly ApplicationDbContext _context;

        public ApartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Apartment apartment)
        {
            _context.Apartments.Add(apartment);
        }

        public async Task<Apartment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Apartments
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }
    }
}
