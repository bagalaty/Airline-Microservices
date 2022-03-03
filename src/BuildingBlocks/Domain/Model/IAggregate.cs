using BuildingBlocks.Domain.Event;

namespace BuildingBlocks.Domain.Model
{
    public interface IAggregate
    {
        IReadOnlyList<IDomainEvent> DomainEvents { get; }
        void AddDomainEvent(IDomainEvent domainEvent);
        void RemoveDomainEvent(IDomainEvent domainEvent);
        void ClearDomainEvents();
    }
}
