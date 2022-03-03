using BuildingBlocks.Domain.Event;

namespace BuildingBlocks.Domain;

public interface IEventProcessor
{
    Task ProcessAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
    Task ProcessAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
    public Task PublishAsync(IEnumerable<IIntegrationEvent> events, CancellationToken cancellationToken = default);
    public Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default);

}
