using Bookify.Domain.Shared;
using FluentAssertions;

namespace Domain.UnitTests.Shared
{
    public class CurrencyTests
    {
        [Theory]
        [InlineData("USD")]
        [InlineData("EUR")]
        public void FromCode_ShouldReturnTheCurrency_WhenCodeIsSupported(string code)
        {
            var currency = Currency.FromCode(code);

            currency.Should().NotBeNull();
            currency!.Code.Should().Be(code);
        }

        [Theory]
        [InlineData("GBP")]
        [InlineData("usd")]   // codes are case-sensitive
        [InlineData("")]
        public void FromCode_ShouldReturnNull_WhenCodeIsUnknown(string code)
        {
            Currency.FromCode(code).Should().BeNull();
        }

        [Fact]
        public void All_ShouldContainTheSupportedCurrencies()
        {
            Currency.All.Should().HaveCount(2);
            Currency.All.Should().Contain(Currency.Usd);
            Currency.All.Should().Contain(Currency.Eur);
        }
    }
}
