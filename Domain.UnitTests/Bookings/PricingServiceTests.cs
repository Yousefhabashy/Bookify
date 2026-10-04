using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Shared;
using Domain.UnitTests.Apartments;
using FluentAssertions;

namespace Domain.UnitTests.Bookings
{
    public class PricingServiceTests
    {
        private readonly PricingService _pricingService = new();
        private static DateRange ThreeNights() => BookingData.ValidPeriod();

        [Fact]
        public void CalculatePrice_ShouldMultiplyNightlyPriceByNights()
        {
            var apartment = ApartmentData.CreateApartment(price: new Money(10m, Currency.Usd));

            var result = _pricingService.CalculatePrice(apartment, ThreeNights());

            result.IsSuccess.Should().BeTrue();
            result.Value.PriceForPeriod.Should().Be(new Money(30m, Currency.Usd));
            result.Value.TotalPrice.Should().Be(new Money(30m, Currency.Usd));
        }

        [Fact]
        public void CalculatePrice_ShouldAddCleaningFee()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(10m, Currency.Usd),
                cleaningFee: new Money(20m, Currency.Usd));

            var result = _pricingService.CalculatePrice(apartment, ThreeNights());

            result.Value.CleaningFee.Should().Be(new Money(20m, Currency.Usd));
            result.Value.TotalPrice.Should().Be(new Money(50m, Currency.Usd));
        }

        [Theory]
        [InlineData(Amenity.GardenView, 5)]
        [InlineData(Amenity.MountainView, 5)]
        [InlineData(Amenity.AirConditioning, 1)]
        [InlineData(Amenity.Parking, 1)]
        [InlineData(Amenity.WiFi, 0)]
        [InlineData(Amenity.PetFriendly, 0)]
        [InlineData(Amenity.SwimmingPool, 0)]
        [InlineData(Amenity.Gym, 0)]
        [InlineData(Amenity.Spa, 0)]
        [InlineData(Amenity.Terrace, 0)]
        public void CalculatePrice_ShouldApplyTheAmenityPercentage(Amenity amenity, int percent)
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(100m, Currency.Usd),
                amenities: new List<Amenity> { amenity });

            var result = _pricingService.CalculatePrice(apartment, ThreeNights());

            var expectedUpCharge = 300m * percent / 100m;
            result.Value.AmenitiesUpCharge.Should().Be(new Money(expectedUpCharge, Currency.Usd));
            result.Value.TotalPrice.Should().Be(new Money(300m + expectedUpCharge, Currency.Usd));
        }

        [Fact]
        public void CalculatePrice_ShouldSumThePercentagesOfAllAmenities()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(100m, Currency.Usd),
                amenities: new List<Amenity> { Amenity.GardenView, Amenity.Parking });

            var result = _pricingService.CalculatePrice(apartment, ThreeNights());

            result.Value.AmenitiesUpCharge.Should().Be(new Money(18m, Currency.Usd));
        }

        [Fact]
        public void CalculatePrice_ShouldCombineAllPriceParts()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(100m, Currency.Usd),
                cleaningFee: new Money(20m, Currency.Usd),
                amenities: new List<Amenity> { Amenity.GardenView, Amenity.AirConditioning });

            var result = _pricingService.CalculatePrice(apartment, ThreeNights());

            result.Value.PriceForPeriod.Should().Be(new Money(300m, Currency.Usd));
            result.Value.CleaningFee.Should().Be(new Money(20m, Currency.Usd));
            result.Value.AmenitiesUpCharge.Should().Be(new Money(18m, Currency.Usd));
            result.Value.TotalPrice.Should().Be(new Money(338m, Currency.Usd));
        }

        [Fact]
        public void CalculatePrice_ShouldUseTheApartmentCurrency()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(100m, Currency.Eur),
                cleaningFee: new Money(10m, Currency.Eur));

            var result = _pricingService.CalculatePrice(apartment, ThreeNights());

            result.Value.TotalPrice.Currency.Should().Be(Currency.Eur);
        }

        [Fact]
        public void CalculatePrice_ShouldFail_WhenCleaningFeeCurrencyDiffersFromPriceCurrency()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(100m, Currency.Usd),
                cleaningFee: new Money(10m, Currency.Eur));

            var result = _pricingService.CalculatePrice(apartment, ThreeNights());

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(MoneyErrors.CurrencyMismatch);
        }

        [Fact]
        public void CalculatePrice_ShouldFail_WhenZeroCleaningFeeHasADifferentCurrency()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(100m, Currency.Usd),
                cleaningFee: Money.Zero(Currency.Eur));

            var result = _pricingService.CalculatePrice(apartment, ThreeNights());

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(MoneyErrors.CurrencyMismatch);
        }

        [Fact]
        public void CalculatePrice_ShouldCopyTheCleaningFee_InsteadOfSharingTheApartmentsInstance()
        {
            var apartment = ApartmentData.CreateApartment(
                price: new Money(100m, Currency.Usd),
                cleaningFee: new Money(20m, Currency.Usd));

            var result = _pricingService.CalculatePrice(apartment, ThreeNights());

            result.Value.CleaningFee.Should().Be(apartment.CleaningFee);          
            result.Value.CleaningFee.Should().NotBeSameAs(apartment.CleaningFee); 
        }
    }
}