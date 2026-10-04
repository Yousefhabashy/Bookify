using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Shared;
using Bookify.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Configurations
{
    internal sealed class BookingConfigurations : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.HasKey(booking => booking.Id);

            builder.OwnsOne(booking => booking.PriceForPeriod, priceBuilder =>
            {
                priceBuilder.Property(money => money.Amount).HasPrecision(18, 2);
                priceBuilder.Property(money => money.Currency)
                    .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
            });

            builder.OwnsOne(booking => booking.CleaningFee, priceBuilder =>
            {
                priceBuilder.Property(money => money.Amount).HasPrecision(18, 2);
                priceBuilder.Property(money => money.Currency)
                    .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
            });

            builder.OwnsOne(booking => booking.AmenitiesUpCharge, priceBuilder =>
            {
                priceBuilder.Property(money => money.Amount).HasPrecision(18, 2);
                priceBuilder.Property(money => money.Currency)
                    .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
            });

            builder.OwnsOne(booking => booking.TotalPrice, priceBuilder =>
            {
                priceBuilder.Property(money => money.Amount).HasPrecision(18, 2);
                priceBuilder.Property(money => money.Currency)
                    .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
            });

            builder.OwnsOne(booking => booking.Duration);

            builder.HasOne<Apartment>()
                .WithMany()
                .HasForeignKey(booking => booking.ApartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(booking => booking.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property<uint>("xmin")
                .IsRowVersion();
        }
    }
}