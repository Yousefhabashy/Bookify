using Bookify.Domain.Abstractions;

namespace Domain.UnitTests.Infrastructure
{
    public abstract class BaseTest
    {
        public static T AssertDomainEventWasPublished<T>(Entity entity)
            where T : IDomainEvent
        {
            var domainEvent = entity.GetDomainEvents().OfType<T>().SingleOrDefault();

            if (domainEvent is null)
            {
                throw new Exception($"Expected domain event of type {typeof(T).Name} was not published.");
            }

            return domainEvent;
        }

        public static void AssertNoDomainEvents(Entity entity)
        {
            var events = entity.GetDomainEvents();

            if (events.Count > 0)
            {
                var names = string.Join(", ", events.Select(e => e.GetType().Name));
                throw new Exception($"Expected no domain events but found: {names}.");
            }
        }
    }
}
