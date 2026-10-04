namespace Domain.UnitTests.Infrastructure
{
    /// <summary>
    /// Fixed dates for all tests. Tests must never read the real clock,
    /// otherwise they start failing once "today" moves past the hard-coded dates.
    /// </summary>
    internal static class TestDates
    {
        // The moment the booking is created.
        public static readonly DateTime Now = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        // A stay of 3 nights, in the future relative to Now.
        public static readonly DateOnly CheckIn = new(2026, 1, 10);
        public static readonly DateOnly CheckOut = new(2026, 1, 13);

        public static DateTime AtStartOfDay(DateOnly date) =>
            date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        public static DateTime AtEndOfDay(DateOnly date) =>
            date.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc);
    }
}
