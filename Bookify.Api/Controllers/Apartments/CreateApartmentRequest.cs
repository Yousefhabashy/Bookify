namespace Bookify.Api.Controllers.Apartments
{
    public sealed record CreateApartmentRequest(
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
        );
}
