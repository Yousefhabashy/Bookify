using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Reviews;
using Bookify.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Configurations
{
    internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("reviews");

            builder.HasKey(review => review.Id);

            builder.Property(review => review.Rating)
                .HasConversion(rating => rating.Value, value => Rating.Create(value).Value);

            builder.Property(review => review.Comment)
                .HasMaxLength(400)
                .HasConversion(comment => comment.Value, value => Comment.Create(value).Value);

            builder.HasOne<Apartment>().WithMany().HasForeignKey(review => review.ApartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Booking>().WithMany().HasForeignKey(review => review.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>().WithMany().HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(review => review.BookingId)
                .IsUnique();

            builder.Property<uint>("xmin")
                .IsRowVersion();
        }
    }
}