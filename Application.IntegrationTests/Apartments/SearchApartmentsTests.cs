using Application.IntegrationTests.Infrastructure;
using Bookify.Application.Apartments.SearchApartments;
using Bookify.Application.Bookings.CancelBooking;
using FluentAssertions;

namespace Application.IntegrationTests.Apartments
{
    public class SearchApartmentsTests : BaseIntegrationTest
    {
        public SearchApartmentsTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Search_ShouldHideABookedApartment_AndShowItAgainAfterTheBookingIsCancelled()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            var bookingId = await ReserveAsync(apartmentId, PeriodFromToday(30, 3));

            var whileBooked = await Sender.Send(new SearchApartmentsQuery(PeriodFromToday(31, 1)));

            whileBooked.IsSuccess.Should().BeTrue();
            whileBooked.Value.Select(a => a.Id).Should().NotContain(apartmentId);

            var cancel = await Sender.Send(new CancelBookingCommand(bookingId));
            cancel.IsSuccess.Should().BeTrue();

            var afterCancel = await Sender.Send(new SearchApartmentsQuery(PeriodFromToday(31, 1)));

            afterCancel.Value.Select(a => a.Id).Should().Contain(apartmentId);
        }

        [Fact]
        public async Task Search_ShouldReturnTheApartment_WhenTheRequestedDatesAreFree()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            await ReserveAsync(apartmentId, PeriodFromToday(30, 3));

            var result = await Sender.Send(new SearchApartmentsQuery(PeriodFromToday(60, 2)));

            result.IsSuccess.Should().BeTrue();
            result.Value.Select(a => a.Id).Should().Contain(apartmentId);
        }
    }
}