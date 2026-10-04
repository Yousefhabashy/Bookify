using Bookify.Application.Abstractions.Apartments.GetApartmentById;
using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Apartments.GetApartmentById
{
    public sealed record GetApartmentByIdQuery(Guid apartmentId) : IQuery<ApartmentResponse>;
}
