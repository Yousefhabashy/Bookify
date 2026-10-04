using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Apartments.CreateApartment
{
    public sealed record CreateApartmentCommand(
        string Name,
        string Description,
        string Country,
        string State,
        string City,
        string Street,
        string ZipCode,
        decimal PriceAmount,
        string PriceCurrency,
        decimal CleaningFeeAmount,
        string CleaningFeeCurrency,
        List<int> Amenities
        ) : ICommand<Guid>;
}
