using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Apartments
{
    public static class ApartmentErrors
    {
        public static readonly Error NotFound = new(
                "Apartment.NotFound",
                "The apartment with the specified identifier was not found",
                ErrorType.NotFound
            );

        public static readonly Error InvalidCurrency = new(
                "Apartment.InvalidCurrency",
                "The provided currencies must match",
                ErrorType.Validation
            );
        public static readonly Error InvalidAmenity = new(
                "Apartment.InvalidAmenity",
                "One or more provided amenities are not supported.",
                ErrorType.Validation
            );
    }
}
