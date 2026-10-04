using Bookify.Domain.Apartments;
using Bookify.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Configurations
{
    internal sealed class ApartmentConfigurations : IEntityTypeConfiguration<Apartment>
    {
        public void Configure(EntityTypeBuilder<Apartment> builder)
        {
            builder.ToTable("apartments");

            builder.HasKey(apartment => apartment.Id);

            builder.Property(apartment => apartment.Name)
                .HasMaxLength(200)
                .HasConversion(name => name.Value, value => Name.Create(value).Value);

            builder.Property(apartment => apartment.Description)
                .HasMaxLength(2000)
                .HasConversion(description => description.Value, value => Description.Create(value).Value);

            builder.OwnsOne(apartment => apartment.Address);

            builder.OwnsOne(apartment => apartment.Price, priceBuilder =>
            {
                priceBuilder.Property(money => money.Amount).HasPrecision(18, 2);

                priceBuilder.Property(money => money.Currency)
                    .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
            });

            builder.OwnsOne(apartment => apartment.CleaningFee, priceBuilder =>
            {
                priceBuilder.Property(money => money.Amount).HasPrecision(18, 2);

                priceBuilder.Property(money => money.Currency)
                    .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
            });

            builder.Property(apartment => apartment.Amenities)
                .HasField("_amenities")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}