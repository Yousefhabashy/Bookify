using FluentValidation;

namespace Bookify.Application.Bookings.RejectBooking
{
    internal sealed class RejectBookingCommandValidator : AbstractValidator<RejectBookingCommand>
    {
        public RejectBookingCommandValidator()
        {
            RuleFor(c => c.BookingId).NotEmpty();
        }
    }
}
