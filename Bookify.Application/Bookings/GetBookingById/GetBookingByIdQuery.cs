using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Bookings.GetBookingById
{
    public sealed record GetBookingByIdQuery(Guid Id) : IQuery<BookingResponse>;
}
