using Bookify.Application.Abstractions.Messaging;
using Bookify.Application.Data;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Apartments.SearchApartments
{
    internal sealed class SearchApartmentsQueryHandler : IQueryHandler<SearchApartmentsQuery, IReadOnlyList<ApartmentResponse>>
    {
        private readonly IApplicationDbContext _context;

        public SearchApartmentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<IReadOnlyList<ApartmentResponse>>> Handle(SearchApartmentsQuery request, CancellationToken cancellationToken)
        {
            List<BookingStatus> activeBookingStatuses = new()
            {
                BookingStatus.Reserved,
                BookingStatus.Confirmed,
                BookingStatus.Completed
            };

            var apartments = await _context.Apartments
                .Where(apartment => !_context.Bookings
                    .Any(booking =>
                        booking.ApartmentId == apartment.Id &&
                        booking.Duration.Start <= request.Range.End &&
                        booking.Duration.End >= request.Range.Start &&
                        activeBookingStatuses.Contains(booking.Status)))
                .Select(a => new ApartmentResponse
                {
                    Id = a.Id,
                    Name = a.Name.Value,
                    Description = a.Description.Value,
                    Country = a.Address.Country,
                    City = a.Address.City,
                    Street = a.Address.Street,
                    Price = a.Price.Amount,
                    Currency = a.Price.Currency.Code
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // even if empty will return  a success with empty list 
            return apartments;
        }
    }
}
