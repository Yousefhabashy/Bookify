using Bookify.Domain.Abstractions;
using MediatR;
using System.Collections.Concurrent;

namespace Bookify.Application.Abstractions.Events
{
    public static class DomainEventNotificationFactory
    {
        // ConcurrentDictionary --> For Thread Safety 
        private static readonly ConcurrentDictionary<Type, Func<IDomainEvent, INotification>> _factories = new();

        public static INotification Wrap(IDomainEvent domainEvent)
        {
            var factory = _factories.GetOrAdd(domainEvent.GetType(), CreateFactory);
            return factory(domainEvent);
        }

        private static Func<IDomainEvent, INotification> CreateFactory(Type domainEventType)
        {
            var wrapperType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEventType);

            var constructor = wrapperType.GetConstructor(
                new[]
                {
                    domainEventType
                }
                )!;

            return domainEvent => (INotification)constructor.Invoke(new object[] { domainEvent });
        }
    }
}