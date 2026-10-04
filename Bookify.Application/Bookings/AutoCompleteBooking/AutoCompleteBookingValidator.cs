using FluentValidation;

namespace Bookify.Application.Bookings.AutoCompleteBooking
{
    internal sealed class AutoCompleteBookingValidator : AbstractValidator<AutoCompleteBookingCommand>
    {
        public AutoCompleteBookingValidator()
        {
            RuleFor(c => c.BookingId).NotEmpty();
        }
    }
}
