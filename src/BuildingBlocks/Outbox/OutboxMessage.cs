using BuildingBlocks.Domain.Event;

namespace BuildingBlocks.Outbox;

public class OutboxMessage
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets name of message.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the date the message occurred.
    /// </summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    /// Gets the event type full name.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the event data - serialized to JSON.
    /// </summary>
    public string Data { get; private set; }

    /// <summary>
    /// Gets the date the message processed.
    /// </summary>
    public DateTime? ProcessedOn { get; private set; }

    /// <summary>
    /// Gets the type of our event.
    /// </summary>
    public EventType EventType { get; private set; }

    /// <summary>
    /// Gets the CorrelationId of our event.
    /// </summary>
    public Guid? CorrelationId { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxMessage"/> class.
    /// Initializes a new outbox message.
    /// </summary>
    /// <param name="id">The outbox message identifier.</param>
    /// <param name="occurredOn">The outbox message date occurred on.</param>
    /// <param name="type">The outbox message type.</param>
    /// <param name="name">The name of event type with underscore naming.</param>
    /// <param name="data">The outbox message data.</param>
    /// <param name="eventType">The outbox event type.</param>
    /// <param name="correlationId">The correlationId of our outbox event.</param>
    public OutboxMessage(
        Guid id,
        DateTime occurredOn,
        string type,
        string name,
        string data,
        EventType eventType,
        Guid? correlationId = null)
    {
        OccurredOn = occurredOn;
        Type = type;
        Data = data;
        Id = id;
        Name = name;
        EventType = eventType;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Sets outbox message process date.
    /// </summary>
    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.Now;
    }

    public bool Validate()
    {
        if (Id == Guid.Empty)
        {
            throw new System.ComponentModel.DataAnnotations.ValidationException(
                "Id of the Outbox entity couldn't be null.");
        }

        if (string.IsNullOrEmpty(Type))
        {
            throw new System.ComponentModel.DataAnnotations.ValidationException(
                "Type of the Outbox entity couldn't be null or empty.");
        }

        if (Data is null)
        {
            throw new System.ComponentModel.DataAnnotations.ValidationException(
                "Payload of the Outbox entity couldn't be null (should be an Avro format).");
        }

        return true;
    }
}
