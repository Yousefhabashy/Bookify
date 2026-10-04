using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Events;
using Bookify.Application.Data;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Reviews;
using Bookify.Domain.Users;
using Bookify.Infrastructure.Abstractions.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Bookify.Infrastructure.Data
{
    public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IDateTimeProvider dateTimeProvider
            )
            : base(options)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public DbSet<Apartment> Apartments => Set<Apartment>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddDomainEventsAsOutboxMessages();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void AddDomainEventsAsOutboxMessages()
        {
            var entities = ChangeTracker
                .Entries<Entity>()
                .Select(entry => entry.Entity)
                .ToList();

            var messages = entities
                .SelectMany(entity => entity.GetDomainEvents())
                .Select(domainEvent => new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = DomainEventRegistry.GetTypeName(domainEvent.GetType()),
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOnUtc = _dateTimeProvider.UtcNow
                })
                .ToList();

            entities.ForEach(entity => entity.ClearDomainEvents());

            OutboxMessages.AddRange(messages);
        }
    }
}
