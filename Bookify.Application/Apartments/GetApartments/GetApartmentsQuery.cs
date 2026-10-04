using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Apartments.GetApartments
{
    public sealed record GetApartmentsQuery() : IQuery<IReadOnlyList<ApartmentResponse>>;
}
