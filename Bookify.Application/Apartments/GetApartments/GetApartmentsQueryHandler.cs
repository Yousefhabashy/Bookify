using Bookify.Application.Abstractions.Messaging;
using Bookify.Application.Data;
using Bookify.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Apartments.GetApartments
{
    internal sealed class GetApartmentsQueryHandler : IQueryHandler<GetApartmentsQuery, IReadOnlyList<ApartmentResponse>>
    {
        private readonly IApplicationDbContext _context;
        public GetApartmentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<IReadOnlyList<ApartmentResponse>>> Handle(GetApartmentsQuery request, CancellationToken cancellationToken)
        {
            var apartments = await _context.Apartments
                .Select(
                    a => new ApartmentResponse
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
