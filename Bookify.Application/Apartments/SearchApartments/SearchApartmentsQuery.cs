using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Bookings;

namespace Bookify.Application.Apartments.SearchApartments
{
    public sealed record SearchApartmentsQuery(DateRange Range) : IQuery<IReadOnlyList<ApartmentResponse>>;
}
