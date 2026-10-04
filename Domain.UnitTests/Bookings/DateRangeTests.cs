using Bookify.Domain.Bookings;
using FluentAssertions;

namespace Domain.UnitTests.Bookings
{
    public class DateRangeTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenStartIsBeforeEnd()
        {
            var start = new DateOnly(2026, 1, 10);
            var end = new DateOnly(2026, 1, 13);

            var result = DateRange.Create(start, end);

            result.IsSuccess.Should().BeTrue();
            result.Value.Start.Should().Be(start);
            result.Value.End.Should().Be(end);
        }

        [Fact]
        public void Create_ShouldFail_WhenStartEqualsEnd()
        {
            var date = new DateOnly(2026, 1, 10);

            var result = DateRange.Create(date, date);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(BookingErrors.DurationIsInvalid);
        }

        [Fact]
        public void Create_ShouldFail_WhenStartIsAfterEnd()
        {
            var result = DateRange.Create(new DateOnly(2026, 1, 13), new DateOnly(2026, 1, 10));

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("DateRange.InvalidDuration");
        }

        [Theory]
        [InlineData(2026, 1, 10, 2026, 1, 11, 1)]
        [InlineData(2026, 1, 10, 2026, 1, 13, 3)]
        [InlineData(2026, 1, 30, 2026, 2, 2, 3)]     // crosses a month boundary
        [InlineData(2026, 12, 30, 2027, 1, 2, 3)]    // crosses a year boundary
        public void LengthInDays_ShouldBeTheNumberOfNights(
            int startYear, int startMonth, int startDay,
            int endYear, int endMonth, int endDay,
            int expectedNights)
        {
            var range = DateRange.Create(
                new DateOnly(startYear, startMonth, startDay),
                new DateOnly(endYear, endMonth, endDay)).Value;

            range.LengthInDays.Should().Be(expectedNights);
        }

        [Fact]
        public void DateRange_ShouldHaveValueEquality()
        {
            var first = DateRange.Create(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 13)).Value;
            var second = DateRange.Create(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 13)).Value;

            first.Should().Be(second);
        }
    }
}
