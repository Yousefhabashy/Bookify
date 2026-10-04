using FluentValidation;

namespace Bookify.Application.Apartments.CreateApartment
{
    internal sealed class CreateApartmentCommandValidator : AbstractValidator<CreateApartmentCommand>
    {
        public CreateApartmentCommandValidator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
            RuleFor(c => c.Description).NotEmpty().MaximumLength(2000);
            RuleFor(c => c.Country).NotEmpty();
            RuleFor(c => c.City).NotEmpty();
            RuleFor(c => c.Street).NotEmpty();
            RuleFor(c => c.State).NotEmpty();
            RuleFor(c => c.ZipCode).NotEmpty().Matches(@"^\d{5}(-\d{4})?$").WithMessage("Zip code must be in the format 12345 or 12345-6789");

            RuleFor(c => c.PriceAmount).GreaterThan(0);
            RuleFor(c => c.PriceCurrency).NotEmpty().Length(3);

            RuleFor(c => c.CleaningFeeAmount).GreaterThanOrEqualTo(0);
            RuleFor(c => c.CleaningFeeCurrency).NotEmpty().Length(3);

            RuleFor(c => c.Amenities).NotNull();
        }
    }
}
