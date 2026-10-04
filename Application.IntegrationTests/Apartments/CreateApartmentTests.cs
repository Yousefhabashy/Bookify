using Application.IntegrationTests.Infrastructure;
using Bookify.Application.Abstractions;
using Bookify.Application.Apartments.CreateApartment;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Apartments
{
    public class CreateApartmentTests : BaseIntegrationTest
    {
        public CreateApartmentTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        private static CreateApartmentCommand ValidCommand(string zipCode = "11511") => new(
            Name: "Nile View Apartment",
            Description: "Quiet place near the river.",
            Country: "Egypt",
            State: "Cairo",
            City: "Cairo",
            Street: "Corniche St",
            ZipCode: zipCode,
            PriceAmount: 120.50m,
            PriceCurrency: "USD",
            CleaningFeeAmount: 15m,
            CleaningFeeCurrency: "USD",
            Amenities: new List<int> { 1, 3, 3, 9 });

        [Fact]
        public async Task CreateApartment_ShouldPersistEverythingCorrectly_WhenCommandIsValid()
        {
            var result = await Sender.Send(ValidCommand());

            result.IsSuccess.Should().BeTrue();

            var saved = await DbContext.Apartments
                .AsNoTracking()
                .SingleAsync(a => a.Id == result.Value);

            saved.Name.Value.Should().Be("Nile View Apartment");
            saved.Address.Country.Should().Be("Egypt");
            saved.Address.City.Should().Be("Cairo");
            saved.Address.Street.Should().Be("Corniche St");
            saved.Price.Amount.Should().Be(120.50m);
            saved.Price.Currency.Code.Should().Be("USD");
            saved.CleaningFee.Amount.Should().Be(15m);

            saved.Amenities.Should().BeEquivalentTo(
                new[] { Amenity.WiFi, Amenity.Parking, Amenity.MountainView });
        }

        [Fact]
        public async Task CreateApartment_ShouldReturnValidationErrorAndSaveNothing_WhenZipCodeIsInvalid()
        {
            var uniqueName = $"Invalid zip {Guid.NewGuid():N}";
            var command = ValidCommand(zipCode: "ABC") with { Name = uniqueName };

            var result = await Sender.Send(command);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ValidationError>();

            var name = Name.Create(uniqueName).Value;
            var exists = await DbContext.Apartments.AnyAsync(a => a.Name == name);
            exists.Should().BeFalse();
        }
    }
}