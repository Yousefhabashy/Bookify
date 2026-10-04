using Bookify.Domain.Bookings.Events;
using Bookify.Domain.Reviews.Events;
using Bookify.Domain.Users.Events;

namespace Bookify.Infrastructure.Abstractions.Outbox
{
    public static class DomainEventRegistry
    {
        private static readonly Dictionary<string, Type> EventTypes = new()
        {
            { "booking-reserved", typeof(BookingReservedDomainEvent) },
            { "booking-rejected", typeof(BookingRejectedDomainEvent) },
            { "booking-confirmed", typeof(BookingConfirmedDomainEvent) },
            { "booking-cancelled", typeof(BookingCancelledDomainEvent) },
            { "booking-completed", typeof(BookingCompletedDomainEvent) },
            { "user-created", typeof(UserCreatedDomainEvent) },
            { "review-created", typeof(ReviewCreatedDomainEvent) }
        };

        private static readonly Dictionary<Type, string> NamesByType =
            EventTypes.ToDictionary(kv => kv.Value, kv => kv.Key);

        public static Type? GetEventType(string name) =>
            EventTypes.GetValueOrDefault(name);

        public static string GetTypeName(Type type) =>
            NamesByType.TryGetValue(type, out var name)
                ? name
                : throw new InvalidOperationException(
                    $"Domain event '{type.Name}' is not registered in DomainEventRegistry.");
    }
}
