using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Event;

namespace BuildingBlocks.EventBus.Messages;

public record FlightCreated(string FlightNumber) : IIntegrationEvent;
