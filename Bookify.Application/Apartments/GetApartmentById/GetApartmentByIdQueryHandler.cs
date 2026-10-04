using Bookify.Application.Abstractions.Apartments.GetApartmentById;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Application.Data;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Apartments.GetApartmentById
{
    internal sealed class GetApartmentByIdQueryHandler : IQueryHandler<GetApartmentByIdQuery, ApartmentResponse>
    {
        private readonly IApplicationDbContext _context;
        public GetApartmentByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ApartmentResponse>> Handle(GetApartmentByIdQuery query, CancellationToken cancellationToken)
        {
            var apartment = await _context.Apartments
            .Where(a => a.Id == query.apartmentId)
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
            .FirstOrDefaultAsync(cancellationToken);

            if (apartment is null)
            {
                return Result.Failure<ApartmentResponse>(ApartmentErrors.NotFound);
            }

            return apartment;
        }
    }
}
