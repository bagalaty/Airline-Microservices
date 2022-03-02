using MassTransit;
using MediatR;

namespace BuildingBlocks.Domain;

[ExcludeFromTopology]
public interface IIntegrationEvent : IEvent
{
}
