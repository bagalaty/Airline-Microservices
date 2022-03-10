namespace BuildingBlocks.Outbox;

public interface IInboxService
{
    Task<bool> ExistEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
}
