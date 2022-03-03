using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Event;

namespace Flight.Flight.Events.Domain;

public record FlightCreatedDomainEvent(string FlightNumber) : IDomainEvent;
