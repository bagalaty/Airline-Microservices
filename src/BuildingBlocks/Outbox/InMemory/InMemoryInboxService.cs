namespace BuildingBlocks.Outbox.InMemory;

public class InMemoryInboxService : IInboxService
{
    private readonly InMemoryOutboxStore _inMemoryOutboxStore;

    public InMemoryInboxService(InMemoryOutboxStore inMemoryOutboxStore)
    {
        _inMemoryOutboxStore = inMemoryOutboxStore;
    }

    public Task<bool> ExistEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        => Task.FromResult(_inMemoryOutboxStore.Events.Any(x => x.Id == eventId));
}
