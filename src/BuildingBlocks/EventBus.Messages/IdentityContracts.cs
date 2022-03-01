using BuildingBlocks.Domain;

namespace BuildingBlocks.EventBus.Messages;

public record UserCreated(long Id, string Name, string PassportNumber) : IIntegrationEvent;
