using MassTransit;

namespace BuildingBlocks.Domain.Event;

[ExcludeFromTopology]
public interface IIntegrationEvent : IEvent
{
}
