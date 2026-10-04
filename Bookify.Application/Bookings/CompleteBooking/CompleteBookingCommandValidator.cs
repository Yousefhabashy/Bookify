using FluentValidation;

namespace Bookify.Application.Bookings.CompleteBooking
{
    internal sealed class CompleteBookingCommandValidator : AbstractValidator<CompleteBookingCommand>
    {
        public CompleteBookingCommandValidator()
        {
            RuleFor(c => c.BookingId).NotEmpty();
        }
    }
}
