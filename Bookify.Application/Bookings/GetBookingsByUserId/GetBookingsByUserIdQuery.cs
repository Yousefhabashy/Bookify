using Bookify.Application.Abstractions.Messaging;
using Bookify.Application.Bookings.GetBookingByUserId;

namespace Bookify.Application.Bookings.GetBookingsByUserId
{
    public sealed record GetBookingsByUserIdQuery() : IQuery<IReadOnlyList<BookingResponse>>;
}
