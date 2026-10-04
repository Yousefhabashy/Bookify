namespace Bookify.Domain.Abstractions
{
    public abstract class Entity
    {
        private List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        public Guid Id { get; protected set; }

        protected Entity(Guid id)
        {
            Id = id;
        }
        public void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IReadOnlyList<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents.ToList();
        }
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
