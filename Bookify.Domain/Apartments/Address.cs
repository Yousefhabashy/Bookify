using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Apartments
{
    public record Address
    {
        public string Country { get; private set; } = default!;
        public string State { get; private set; } = default!;
        public string ZipCode { get; private set; } = default!;
        public string City { get; private set; } = default!;
        public string Street { get; private set; } = default!;

        private Address(string country, string state, string zipCode, string city, string street)
        {
            Country = country;
            State = state;
            ZipCode = zipCode;
            City = city;
            Street = street;
        }

        public static Result<Address> Create(string country, string state, string zipCode, string city, string street)
        {
            if (string.IsNullOrWhiteSpace(country) ||
                string.IsNullOrWhiteSpace(state) ||
                string.IsNullOrWhiteSpace(zipCode) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(street))
            {
                return Result.Failure<Address>(new Error("Address.Invalid", "All address fields are required."));
            }

            return new Address(country, state, zipCode, city, street);
        }

        private Address() { }
    }
}
