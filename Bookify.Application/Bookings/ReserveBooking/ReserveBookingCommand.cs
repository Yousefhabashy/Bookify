using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Bookings;

namespace Bookify.Application.Bookings.ReserveBooking
{
    public sealed record ReserveBookingCommand(
        Guid ApartmentId,
        DateRange Duration
        ) : ICommand<Guid>;
}
