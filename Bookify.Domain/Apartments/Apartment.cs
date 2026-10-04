using Bookify.Domain.Abstractions;
using Bookify.Domain.Shared;

namespace Bookify.Domain.Apartments
{
    public sealed class Apartment : Entity
    {
        private Apartment() : base(Guid.Empty)
        {
            // Only for EFCore.
        }

        private Apartment(
            Guid id,
            Name name,
            Description description,
            Address address,
            Money price,
            Money cleaningFee,
            List<Amenity> amenities)
            : base(id)
        {
            Name = name;
            Description = description;
            Address = address;
            Price = price;
            CleaningFee = cleaningFee;
            // defencive copy
            _amenities = new List<Amenity>(amenities);
        }

        public Name Name { get; private set; }
        public Description Description { get; private set; }

        public Address Address { get; private set; }

        public Money Price { get; private set; }

        public Money CleaningFee { get; private set; }

        public DateTime? LastBookedOnUtc { get; internal set; }


        // capsulating amenites (separated Backing Field)
        private readonly List<Amenity> _amenities;
        public IReadOnlyList<Amenity> Amenities => _amenities;

        // for changing list (only accessed from Apartment class)
        private void AddAmenity(Amenity amenity)
        {
            _amenities.Add(amenity);
        }

        public static Result<Apartment> Create(
            Guid id,
            Name name,
            Description description,
            Address address,
            Money price,
            Money cleaningFee,
            List<Amenity> amenities)
        {
            if (price.Amount <= 0)
            {
                return Result.Failure<Apartment>(new Error("Apartment.InvalidPrice", "Price must be greater than zero."));
            }

            if (cleaningFee.Amount < 0)
            {
                return Result.Failure<Apartment>(new Error("Apartment.InvalidCleaningFee", "Cleaning fee cannot be negative."));
            }

            if (amenities == null || !amenities.Any())
            {
                return Result.Failure<Apartment>(new Error("Apartment.NoAmenities", "At least one amenity must be provided."));
            }


            var apartmet = new Apartment(id, name, description, address, price, cleaningFee, amenities);
            return apartmet;
        }
    }
}
