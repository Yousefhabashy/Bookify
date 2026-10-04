using Bookify.Domain.Apartments;
using FluentAssertions;

namespace Domain.UnitTests.Apartments
{
    public class AddressTests
    {
        [Fact]
        public void Create_ShouldMapEachArgumentToTheRightProperty()
        {
            // Address.Create(country, state, zipCode, city, street)
            var result = Address.Create("Egypt", "Giza", "12611", "Dokki", "Tahrir St");

            result.IsSuccess.Should().BeTrue();
            result.Value.Country.Should().Be("Egypt");
            result.Value.State.Should().Be("Giza");
            result.Value.ZipCode.Should().Be("12611");
            result.Value.City.Should().Be("Dokki");
            result.Value.Street.Should().Be("Tahrir St");
        }

        [Theory]
        [InlineData("", "Giza", "12611", "Dokki", "Tahrir St")]
        [InlineData("Egypt", "", "12611", "Dokki", "Tahrir St")]
        [InlineData("Egypt", "Giza", "", "Dokki", "Tahrir St")]
        [InlineData("Egypt", "Giza", "12611", "", "Tahrir St")]
        [InlineData("Egypt", "Giza", "12611", "Dokki", "")]
        [InlineData("Egypt", "Giza", "12611", "Dokki", "   ")]
        public void Create_ShouldFail_WhenAnyFieldIsMissing(
            string country, string state, string zipCode, string city, string street)
        {
            var result = Address.Create(country, state, zipCode, city, street);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Address.Invalid");
        }

        [Fact]
        public void Address_ShouldHaveValueEquality()
        {
            var first = Address.Create("Egypt", "Giza", "12611", "Dokki", "Tahrir St").Value;
            var second = Address.Create("Egypt", "Giza", "12611", "Dokki", "Tahrir St").Value;

            first.Should().Be(second);
        }
    }
}
