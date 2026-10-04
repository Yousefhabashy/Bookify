using Bookify.Infrastructure.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Configurations
{
    internal sealed class OutboxMessageConfigurations : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .ValueGeneratedNever();

            builder.Property(o => o.Type)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(o => o.Content)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(o => o.OccurredOnUtc)
                .IsRequired();

            builder.Property(o => o.ProcessedOnUtc);
            builder.HasIndex(o => o.ProcessedOnUtc)
                .HasFilter("processed_on_utc IS NULL");

            builder.Property(o => o.Error)
                .HasColumnType("text");

            builder.Property(o => o.RetryCount)
                .HasDefaultValue(0);
        }
    }
}
