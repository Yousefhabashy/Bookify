using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Bookings
{
    public static class BookingErrors
    {
        public static readonly Error NotFound = new(
                "Booking.NotFound",
                "The booking with the specified identifier was not found",
                ErrorType.NotFound
            );

        public static readonly Error Overlap = new(
                "Booking.Overlap",
                "The current booking is overlapping with an existing one",
                ErrorType.Conflict
            );

        public static readonly Error NotReserved = new(
                "Booking.NotReserved",
                "The booking is not pending",
                ErrorType.Conflict
            );

        public static readonly Error NotConfirmed = new(
                "Booking.NotConfirmed",
                "The booking is not confirmed",
                ErrorType.Conflict
            );

        public static readonly Error AlreadyStarted = new(
                "Booking.AlreadyStarted",
                "The booking has already started",
                ErrorType.Conflict
            );

        public static readonly Error DurationIsInvalid = new(
                "DateRange.InvalidDuration",
                "End date precedes start date",
                ErrorType.Validation
            );

        public static readonly Error NotEnded = new(
                "Booking.NotEnded",
                "The booking has not ended",
                ErrorType.Failure
            );
        public static readonly Error NotCancellable = new(
                "Booking.NotCancellable",
                "Only reserved or confirmed bookings can be cancelled",
                ErrorType.Conflict
            );

        public static readonly Error StartDateInThePast = new(
                "Booking.StartDateInThePast",
                "The booking start date cannot be in the past",
                ErrorType.Validation
            );
    }
}
