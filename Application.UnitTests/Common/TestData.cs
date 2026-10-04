using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Shared;

namespace Application.UnitTests.Common
{
    internal static class TestData
    {
        public static readonly DateTime Now = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        public static readonly DateOnly CheckIn = new(2026, 1, 10);
        public static readonly DateOnly CheckOut = new(2026, 1, 13);

        public static DateTime AtStartOfDay(DateOnly date) =>
            date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        public static Apartment CreateApartment() =>
            Apartment.Create(
                Guid.NewGuid(),
                Name.Create("Test Apartment").Value,
                Description.Create("A nice place to stay.").Value,
                Address.Create("Egypt", "Cairo", "11511", "Cairo", "Tahrir St").Value,
                new Money(100m, Currency.Usd),
                Money.Zero(Currency.Usd),
                new List<Amenity> { Amenity.WiFi }).Value;

        public static Booking CreateBooking(BookingStatus status = BookingStatus.Reserved, Guid? userId = null)
        {
            var period = DateRange.Create(CheckIn, CheckOut).Value;

            var booking = Booking.Reserve(
                CreateApartment(),
                userId ?? Guid.NewGuid(),
                period,
                Now,
                new PricingService()).Value;

            switch (status)
            {
                case BookingStatus.Reserved:
                    break;
                case BookingStatus.Confirmed:
                    booking.Confirm(Now);
                    break;
                case BookingStatus.Rejected:
                    booking.Reject(Now);
                    break;
                case BookingStatus.Cancelled:
                    booking.Cancel(Now);
                    break;
                case BookingStatus.Completed:
                    booking.Confirm(Now);
                    booking.Complete(AtStartOfDay(CheckOut));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status));
            }

            if (booking.Status != status)
            {
                throw new InvalidOperationException($"Could not arrange a booking in status {status}.");
            }

            return booking;
        }
    }
}