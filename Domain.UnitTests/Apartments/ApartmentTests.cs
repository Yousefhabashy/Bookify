using Bookify.Domain.Apartments;
using Bookify.Domain.Shared;
using FluentAssertions;

namespace Domain.UnitTests.Apartments
{
    public class ApartmentTests
    {
        private static Bookify.Domain.Abstractions.Result<Apartment> Create(
            Money? price = null,
            Money? cleaningFee = null,
            List<Amenity>? amenities = null) =>
            Apartment.Create(
                ApartmentData.ValidApartmentId,
                ApartmentData.ValidName,
                ApartmentData.ValidDescription,
                ApartmentData.ValidAddress,
                price ?? ApartmentData.DefaultPrice,
                cleaningFee ?? ApartmentData.DefaultCleaningFee,
                amenities!);

        [Fact]
        public void Create_ShouldSucceed_WhenAllDetailsAreValid()
        {
            var amenities = new List<Amenity> { Amenity.WiFi, Amenity.Parking };

            var result = Create(amenities: amenities);

            result.IsSuccess.Should().BeTrue();
            var apartment = result.Value;
            apartment.Id.Should().Be(ApartmentData.ValidApartmentId);
            apartment.Name.Should().Be(ApartmentData.ValidName);
            apartment.Description.Should().Be(ApartmentData.ValidDescription);
            apartment.Address.Should().Be(ApartmentData.ValidAddress);
            apartment.Price.Should().Be(ApartmentData.DefaultPrice);
            apartment.CleaningFee.Should().Be(ApartmentData.DefaultCleaningFee);
            apartment.Amenities.Should().Equal(amenities);
            apartment.LastBookedOnUtc.Should().BeNull();
        }

        [Fact]
        public void Create_ShouldAcceptAZeroCleaningFee()
        {
            var result = Create(
                cleaningFee: Money.Zero(Currency.Usd),
                amenities: ApartmentData.NoUpChargeAmenities());

            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100.5)]
        public void Create_ShouldFail_WhenPriceIsZeroOrNegative(double amount)
        {
            var result = Create(
                price: new Money((decimal)amount, Currency.Usd),
                amenities: ApartmentData.NoUpChargeAmenities());

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Apartment.InvalidPrice");
        }

        [Fact]
        public void Create_ShouldFail_WhenCleaningFeeIsNegative()
        {
            var result = Create(
                cleaningFee: new Money(-1m, Currency.Usd),
                amenities: ApartmentData.NoUpChargeAmenities());

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Apartment.InvalidCleaningFee");
        }

        [Fact]
        public void Create_ShouldFail_WhenThereAreNoAmenities()
        {
            var result = Create(amenities: new List<Amenity>());

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Apartment.NoAmenities");
        }

        [Fact]
        public void Create_ShouldFail_WhenAmenitiesAreNull()
        {
            var result = Create(amenities: null);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Apartment.NoAmenities");
        }

        [Fact]
        public void Create_ShouldCopyTheAmenities_SoLaterChangesToTheListDoNotLeakIn()
        {
            var amenities = new List<Amenity> { Amenity.WiFi };
            var apartment = Create(amenities: amenities).Value;

            amenities.Add(Amenity.Spa);

            apartment.Amenities.Should().Equal(new List<Amenity> { Amenity.WiFi });
        }
    }
}
