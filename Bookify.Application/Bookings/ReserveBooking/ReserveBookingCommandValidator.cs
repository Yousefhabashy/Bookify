using Bookify.Application.Abstractions.Clock;
using FluentValidation;

namespace Bookify.Application.Bookings.ReserveBooking
{
    internal sealed class ReserveBookingCommandValidator : AbstractValidator<ReserveBookingCommand>
    {

        public ReserveBookingCommandValidator(IDateTimeProvider dateTimeProvider)
        {
            RuleFor(c => c.ApartmentId).NotEmpty();

            RuleFor(c => c.Duration).NotNull();

            RuleFor(c => c.Duration.Start)
                .NotEmpty()
                .WithMessage("Start date is required.")
                .Must(startDate => startDate >= DateOnly.FromDateTime(dateTimeProvider.UtcNow))
                .WithMessage("Start date cannot be in the past.");

            RuleFor(c => c.Duration.End)
                .NotEmpty()
                .WithMessage("End date is required.");

            RuleFor(c => c.Duration)
                .Must(dateRange => dateRange.End > dateRange.Start)
                .WithMessage("End date must be later than start date.");
        }
    }
}
