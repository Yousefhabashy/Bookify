using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Bookings.AutoCompleteBooking
{
    public sealed record AutoCompleteBookingCommand(Guid BookingId) : ICommand;
}
