using FluentValidation;

namespace Bookify.Application.Bookings.AutoRejectBooking
{
    internal sealed class AutoRejectBookingValidator : AbstractValidator<AutoRejectBookingCommand>
    {
        public AutoRejectBookingValidator()
        {
            RuleFor(c => c.BookingId).NotEmpty();
        }
    }
}
