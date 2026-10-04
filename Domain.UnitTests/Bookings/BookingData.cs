using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Domain.UnitTests.Apartments;
using Domain.UnitTests.Infrastructure;

namespace Domain.UnitTests.Bookings
{
    internal static class BookingData
    {
        public static readonly Guid UserId = Guid.NewGuid();

        public static DateRange ValidPeriod() =>
            DateRange.Create(TestDates.CheckIn, TestDates.CheckOut).Value;

        public static Booking CreateReserved(Apartment? apartment = null)
        {
            var result = Booking.Reserve(
                apartment ?? ApartmentData.CreateApartment(),
                UserId,
                ValidPeriod(),
                TestDates.Now,
                new PricingService());

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Test data is invalid: {result.Error.Code}");
            }

            return result.Value;
        }

        /// <summary>
        /// Builds a booking in the requested status using only real domain transitions.
        /// Domain events raised while arranging are cleared, so a test only sees its own events.
        /// </summary>
        public static Booking CreateWithStatus(BookingStatus status)
        {
            var booking = CreateReserved();

            switch (status)
            {
                case BookingStatus.Reserved:
                    break;
                case BookingStatus.Confirmed:
                    booking.Confirm(TestDates.Now);
                    break;
                case BookingStatus.Rejected:
                    booking.Reject(TestDates.Now);
                    break;
                case BookingStatus.Cancelled:
                    booking.Cancel(TestDates.Now);
                    break;
                case BookingStatus.Completed:
                    booking.Confirm(TestDates.Now);
                    booking.Complete(TestDates.AtStartOfDay(TestDates.CheckOut));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status));
            }

            if (booking.Status != status)
            {
                throw new InvalidOperationException($"Could not arrange a booking in status {status}.");
            }

            booking.ClearDomainEvents();
            return booking;
        }
    }
}
