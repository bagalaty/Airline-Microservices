namespace BuildingBlocks.Outbox;

[Flags]
public enum EventType
{
    IntegrationEvent = 1,
    DomainEvent = 2,
}
