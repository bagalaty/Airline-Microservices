namespace BuildingBlocks.Domain;

public interface IEvent
{
    Guid EventId => Guid.NewGuid();
    public Guid CorrelationId => Guid.NewGuid();
    public DateTime OccurredOn => DateTime.Now;
    public string EventType => GetType().AssemblyQualifiedName;
}
