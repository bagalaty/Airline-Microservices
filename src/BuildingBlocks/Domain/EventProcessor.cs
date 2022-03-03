using BuildingBlocks.Domain.Event;
using BuildingBlocks.Outbox;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Domain;

public sealed class EventProcessor : IEventProcessor
{
    private readonly IEventMapper _eventMapper;
    private readonly ILogger<IEventProcessor> _logger;
    private readonly IOutboxService _outboxService;
    private readonly IMediator _mediator;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventProcessor(IServiceScopeFactory serviceScopeFactory, IEventMapper eventMapper,
        ILogger<IEventProcessor> logger,
        IOutboxService outboxService,
        IMediator mediator)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _eventMapper = eventMapper;
        _logger = logger;
        _outboxService = outboxService;
        _mediator = mediator;
    }

    public async Task ProcessAsync(IDomainEvent @event, CancellationToken cancellationToken = default) => await ProcessAsync(new[] { @event }, cancellationToken).ConfigureAwait(false);

    public async Task ProcessAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (events is null) return;

        _logger.LogTrace("Processing domain events start...");
        var integrationEvents = await HandleDomainEventsAsync(events).ConfigureAwait(false);
        _logger.LogTrace("Processing domain events done...");
        if (!integrationEvents.Any()) return;

        _logger.LogTrace("Processing integration events start...");

        await PublishAsync(integrationEvents, cancellationToken).ConfigureAwait(false);

        _logger.LogTrace("Processing integration events done...");
    }

    public async Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default) => await PublishAsync(new[] { @event }, cancellationToken).ConfigureAwait(false);

    public async Task PublishAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken)
    {
        foreach (var integrationEvent in integrationEvents)
            await _outboxService.SaveAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<IIntegrationEvent>> HandleDomainEventsAsync(IEnumerable<IDomainEvent> events)
    {
        var wrappedIntegrationEvents = GetWrappedIntegrationEvents(events.ToList())?.ToList();
        if (wrappedIntegrationEvents?.Count > 0)
            return wrappedIntegrationEvents;

        var integrationEvents = new List<IIntegrationEvent>();
        using var scope = _serviceScopeFactory.CreateScope();
        foreach (var @event in events)
        {
            var eventType = @event.GetType();
            _logger.LogTrace($"Handling domain event: {eventType.Name}");

            await _mediator.Publish(@event).ConfigureAwait(false);

            var integrationEvent = _eventMapper.Map(@event);

            if (integrationEvent is null) continue;

            integrationEvents.Add(integrationEvent);
        }

        return integrationEvents;
    }
    private IEnumerable<IIntegrationEvent> GetWrappedIntegrationEvents(IList<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents.Where(x =>
                     x is IHaveIntegrationEvent))
        {
            var genericType = typeof(IntegrationEventWrapper<>)
                .MakeGenericType(domainEvent.GetType());

            var domainNotificationEvent = (IIntegrationEvent)Activator
                .CreateInstance(genericType, domainEvent);

            yield return domainNotificationEvent;
        }
    }
}
