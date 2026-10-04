using Bookify.Domain.Abstractions;
using FluentAssertions;

namespace Domain.UnitTests.Abstractions
{
    public class EntityTests
    {
        private sealed record TestEvent : IDomainEvent;

        private sealed class TestEntity : Entity
        {
            public TestEntity(Guid id) : base(id) { }
        }

        [Fact]
        public void NewEntity_ShouldHaveNoDomainEvents()
        {
            var entity = new TestEntity(Guid.NewGuid());

            entity.GetDomainEvents().Should().BeEmpty();
        }

        [Fact]
        public void RaiseDomainEvent_ShouldAddTheEvent()
        {
            var entity = new TestEntity(Guid.NewGuid());
            var domainEvent = new TestEvent();

            entity.RaiseDomainEvent(domainEvent);

            entity.GetDomainEvents().Should().HaveCount(1);
            entity.GetDomainEvents()[0].Should().Be(domainEvent);
        }

        [Fact]
        public void ClearDomainEvents_ShouldRemoveAllEvents()
        {
            var entity = new TestEntity(Guid.NewGuid());
            entity.RaiseDomainEvent(new TestEvent());
            entity.RaiseDomainEvent(new TestEvent());

            entity.ClearDomainEvents();

            entity.GetDomainEvents().Should().BeEmpty();
        }

        [Fact]
        public void GetDomainEvents_ShouldReturnACopy()
        {
            var entity = new TestEntity(Guid.NewGuid());
            entity.RaiseDomainEvent(new TestEvent());

            var snapshot = entity.GetDomainEvents();
            entity.ClearDomainEvents();

            // The earlier snapshot is not affected by clearing the entity's list.
            snapshot.Should().HaveCount(1);
        }

        [Fact]
        public void Constructor_ShouldSetId()
        {
            var id = Guid.NewGuid();

            var entity = new TestEntity(id);

            entity.Id.Should().Be(id);
        }
    }
}
