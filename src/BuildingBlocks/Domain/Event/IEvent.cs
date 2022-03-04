using MassTransit;

namespace BuildingBlocks.Domain.Event;

public interface IEvent
{
    Guid EventId => Guid.NewGuid();
    public DateTime OccurredOn => DateTime.Now;
    public string EventType => GetType().AssemblyQualifiedName;
}
