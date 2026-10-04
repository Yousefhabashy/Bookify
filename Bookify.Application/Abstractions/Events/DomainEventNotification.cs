using Bookify.Domain.Abstractions;
using MediatR;

namespace Bookify.Application.Abstractions.Events
{
    public sealed class DomainEventNotification<TDomainEvent> : INotification
        where TDomainEvent : IDomainEvent
    {
        public DomainEventNotification(TDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }

        public TDomainEvent DomainEvent { get; }
    }
}
