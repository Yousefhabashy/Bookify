using Bookify.Domain.Apartments;
using Bookify.Domain.Shared;

namespace Domain.UnitTests.Apartments
{
    internal static class ApartmentData
    {
        public static readonly Guid ValidApartmentId = Guid.NewGuid();
        public static readonly Name ValidName = Name.Create("Test Apartment").Value;
        public static readonly Description ValidDescription = Description.Create("A nice place to stay.").Value;

        // Address.Create(country, state, zipCode, city, street)
        public static readonly Address ValidAddress =
            Address.Create("Egypt", "Cairo", "11511", "Cairo", "Tahrir St").Value;

        public static readonly Money DefaultPrice = new(100, Currency.Usd);
        public static readonly Money DefaultCleaningFee = Money.Zero(Currency.Usd);

        // WiFi has no price up-charge, so it keeps pricing tests simple.
        public static List<Amenity> NoUpChargeAmenities() => new() { Amenity.WiFi };

        public static Apartment CreateApartment(
            Money? price = null,
            Money? cleaningFee = null,
            List<Amenity>? amenities = null)
        {
            var result = Apartment.Create(
                ValidApartmentId,
                ValidName,
                ValidDescription,
                ValidAddress,
                price ?? DefaultPrice,
                cleaningFee ?? DefaultCleaningFee,
                amenities ?? NoUpChargeAmenities());

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Test data is invalid: {result.Error.Code}");
            }

            return result.Value;
        }
    }
}
