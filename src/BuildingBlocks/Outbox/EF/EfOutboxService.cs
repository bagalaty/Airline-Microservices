using Ardalis.GuardClauses;
using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Event;
using BuildingBlocks.EFCore;
using Humanizer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BuildingBlocks.Outbox.EF;

public class EfOutboxService : IOutboxService
{
    private readonly OutboxOptions _options;
    private readonly ILogger<EfOutboxService> _logger;
    private readonly IPublishEndpoint _pushEndpoint;
    private readonly IDbContext _dbContext;

    public EfOutboxService(
        IOptions<OutboxOptions> options,
        ILogger<EfOutboxService> logger,
        IPublishEndpoint pushEndpoint,
        IDbContext dbContext)
    {
        _options = options.Value;
        _logger = logger;
        _pushEndpoint = pushEndpoint;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<OutboxMessage>> GetAllUnsentOutboxMessagesAsync(
        EventType eventType = EventType.IntegrationEvent | EventType.DomainEvent,
        CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.OutboxMessages
            .Where(x => x.EventType == eventType && x.ProcessedOn == null)
            .ToListAsync(cancellationToken: cancellationToken);

        return messages;
    }

    public async Task<IEnumerable<OutboxMessage>> GetAllOutboxMessagesAsync(
        EventType eventType = EventType.IntegrationEvent | EventType.DomainEvent,
        CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.OutboxMessages
            .Where(x => x.EventType == eventType).ToListAsync(cancellationToken: cancellationToken);

        return messages;
    }

    public async Task CleanProcessedAsync(CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.OutboxMessages
            .Where(x => x.ProcessedOn != null).ToListAsync(cancellationToken: cancellationToken);

        _dbContext.OutboxMessages.RemoveRange(messages);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(integrationEvent, nameof(integrationEvent));

        if (!_options.Enabled)
        {
            _logger.LogWarning("Outbox is disabled, outgoing messages won't be saved");
            return;
        }

        string name = integrationEvent.GetType().Name;

        var outboxMessages = new OutboxMessage(
            integrationEvent.EventId,
            integrationEvent.OccurredOn,
            integrationEvent.EventType,
            name.Underscore(),
            JsonConvert.SerializeObject(integrationEvent),
            EventType.IntegrationEvent,
            integrationEvent.CorrelationId);

        await _dbContext.OutboxMessages.AddAsync(outboxMessages, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved message to the outbox");
    }


    public async Task PublishUnsentOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        var unsentMessages = await _dbContext.OutboxMessages
            .Where(x => x.ProcessedOn == null).ToListAsync(cancellationToken);

        if (!unsentMessages.Any())
        {
            _logger.LogTrace("No unsent messages found in outbox");
            return;
        }

        _logger.LogTrace(
            "Found {Count} unsent messages in outbox, sending...",
            unsentMessages.Count);

        foreach (var outboxMessage in unsentMessages)
        {
            var type = Type.GetType(outboxMessage.Type);

            if (type is null)
                continue;

            dynamic data = JsonConvert.DeserializeObject(outboxMessage.Data, type);
            if (data is null)
            {
                _logger.LogError("Invalid message type: {Name}", type?.Name);
                continue;
            }

            if (outboxMessage.EventType == EventType.IntegrationEvent)
            {
                var integrationEvent = data as IIntegrationEvent;

                // integration event
                await _pushEndpoint.Publish((dynamic)integrationEvent, cancellationToken);

                _logger.LogTrace(
                    "Publish a message: '{Name}' with ID: '{Id} (outbox)'",
                    outboxMessage.Name,
                    integrationEvent?.EventId);
            }

            outboxMessage.MarkAsProcessed();
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
