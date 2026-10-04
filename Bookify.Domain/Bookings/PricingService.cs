using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Bookify.Domain.Shared;

namespace Bookify.Domain.Bookings
{
    public class PricingService
    {
        public Result<PricingDetails> CalculatePrice(Apartment apartment, DateRange period)
        {
            var currency = apartment.Price.Currency;

            var priceForPeriod = new Money(apartment.Price.Amount * period.LengthInDays, currency);

            var percentageUpCharge = apartment.Amenities.Sum(a => a switch
            {
                Amenity.GardenView or Amenity.MountainView => 0.05m,
                Amenity.AirConditioning or Amenity.Parking => 0.01m,
                _ => 0m
            });

            var amenitiesUpCharge = new Money(priceForPeriod.Amount * percentageUpCharge, currency);

            var withCleaningFee = Money.Add(priceForPeriod, apartment.CleaningFee);
            if (withCleaningFee.IsFailure)
            {
                return Result.Failure<PricingDetails>(withCleaningFee.Error);
            }

            var totalPrice = Money.Add(withCleaningFee.Value, amenitiesUpCharge);
            if (totalPrice.IsFailure)
            {
                return Result.Failure<PricingDetails>(totalPrice.Error);
            }

            return new PricingDetails(
                priceForPeriod,
                apartment.CleaningFee with { },
                amenitiesUpCharge,
                totalPrice.Value);
        }
    }
}