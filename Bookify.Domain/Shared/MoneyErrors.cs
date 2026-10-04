using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Shared
{
    public static class MoneyErrors
    {
        public static readonly Error CurrencyMismatch = new(
            "Money.CurrencyMismatch",
            "Currencies have to be equal",
            ErrorType.Validation);
    }
}
