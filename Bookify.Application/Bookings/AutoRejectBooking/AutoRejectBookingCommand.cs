using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Bookings.AutoRejectBooking
{
    public sealed record AutoRejectBookingCommand(Guid BookingId) : ICommand;
}
