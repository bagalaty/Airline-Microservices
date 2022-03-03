using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Event;

namespace BuildingBlocks.EventBus.Messages;

public record UserCreated(long Id, string Name, string PassportNumber) : IIntegrationEvent;
