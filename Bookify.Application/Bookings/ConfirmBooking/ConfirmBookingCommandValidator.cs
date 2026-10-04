using FluentValidation;

namespace Bookify.Application.Bookings.ConfirmBooking
{
    internal sealed class ConfirmBookingCommandValidator : AbstractValidator<ConfirmBookingCommand>
    {
        public ConfirmBookingCommandValidator()
        {
            RuleFor(c => c.BookingId).NotEmpty();
        }
    }
}
