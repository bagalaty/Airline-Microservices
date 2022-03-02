namespace BuildingBlocks.Outbox.InMemory;

public interface IInMemoryOutboxStore
{
    public IList<OutboxMessage> Events { get; }
}
