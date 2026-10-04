using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Bookify.Domain.Shared;

namespace Bookify.Application.Apartments.CreateApartment
{
    internal sealed class CreateApartmentCommandHandler : ICommandHandler<CreateApartmentCommand, Guid>
    {
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateApartmentCommandHandler(IApartmentRepository repository, IUnitOfWork unit)
        {
            _apartmentRepository = repository;
            _unitOfWork = unit;
        }
        public async Task<Result<Guid>> Handle(CreateApartmentCommand request, CancellationToken cancellationToken)
        {
            // Check Currency Code for Price and Cleaning fee 
            var currency = Currency.FromCode(request.PriceCurrency);
            if (currency is null)
                return Result.Failure<Guid>(ApartmentErrors.InvalidCurrency);

            var cleaningFeeCurrency = Currency.FromCode(request.CleaningFeeCurrency);
            if (cleaningFeeCurrency is null)
                return Result.Failure<Guid>(ApartmentErrors.InvalidCurrency);

            if (cleaningFeeCurrency != currency)
                return Result.Failure<Guid>(MoneyErrors.CurrencyMismatch);

            // Check Amenities
            var invalidAmenities = request.Amenities.Where(a => !Enum.IsDefined(typeof(Amenity), a)).ToList();
            if (invalidAmenities.Any())
                return Result.Failure<Guid>(ApartmentErrors.InvalidAmenity);

            var name = Name.Create(request.Name);
            if (name.IsFailure) return Result.Failure<Guid>(name.Error);

            var description = Description.Create(request.Description);
            if (description.IsFailure) return Result.Failure<Guid>(description.Error);

            var address = Address.Create(request.Country, request.State, request.ZipCode, request.City, request.Street);
            if (address.IsFailure) return Result.Failure<Guid>(address.Error);

            // create apartment 
            var apartment = Apartment.Create(
                Guid.NewGuid(),
                name.Value,
                description.Value,
                address.Value,
                new Money(request.PriceAmount, currency),
                new Money(request.CleaningFeeAmount, cleaningFeeCurrency),
                request.Amenities.Select(a => (Amenity)a).Distinct().ToList()
            );

            // chceck failure
            if (apartment.IsFailure)
                return Result.Failure<Guid>(apartment.Error);

            // paas it to repository
            _apartmentRepository.Add(apartment.Value);
            // save
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return apartment.Value.Id;
        }
    }
}
