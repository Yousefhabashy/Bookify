using FluentValidation;

namespace Bookify.Application.Bookings.CancelBooking
{
    internal sealed class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingCommandValidator()
        {
            RuleFor(c => c.BookingId).NotEmpty();
        }
    }
}
