using Bookify.Domain.Shared;
using FluentAssertions;

namespace Domain.UnitTests.Shared
{
    public class MoneyTests
    {
        [Fact]
        public void Add_ShouldSumAmounts_WhenCurrenciesMatch()
        {
            var first = new Money(10.50m, Currency.Usd);
            var second = new Money(4.25m, Currency.Usd);

            var result = Money.Add(first, second);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(new Money(14.75m, Currency.Usd));
        }

        [Fact]
        public void Add_ShouldFail_WhenCurrenciesDiffer()
        {
            var usd = new Money(10, Currency.Usd);
            var eur = new Money(10, Currency.Eur);

            var result = Money.Add(usd, eur);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(MoneyErrors.CurrencyMismatch);
        }

        [Fact]
        public void Zero_WithCurrency_ShouldHaveZeroAmountAndThatCurrency()
        {
            var zero = Money.Zero(Currency.Eur);

            zero.Amount.Should().Be(0m);
            zero.Currency.Should().Be(Currency.Eur);
        }

        [Fact]
        public void Zero_WithoutCurrency_ShouldHaveEmptyCurrencyCode()
        {
            var zero = Money.Zero();

            zero.Amount.Should().Be(0m);
            zero.Currency.Code.Should().BeEmpty();
        }

        [Fact]
        public void IsZero_ShouldBeTrue_WhenAmountIsZero()
        {
            new Money(0, Currency.Usd).IsZero().Should().BeTrue();
        }

        [Fact]
        public void IsZero_ShouldBeFalse_WhenAmountIsNotZero()
        {
            new Money(0.01m, Currency.Usd).IsZero().Should().BeFalse();
        }

        [Fact]
        public void Money_ShouldHaveValueEquality()
        {
            var first = new Money(10, Currency.Usd);
            var second = new Money(10, Currency.Usd);

            first.Should().Be(second);
            (first == second).Should().BeTrue();
        }

        [Fact]
        public void Money_ShouldNotBeEqual_WhenCurrencyDiffers()
        {
            var usd = new Money(10, Currency.Usd);
            var eur = new Money(10, Currency.Eur);

            usd.Should().NotBe(eur);
        }
    }
}
