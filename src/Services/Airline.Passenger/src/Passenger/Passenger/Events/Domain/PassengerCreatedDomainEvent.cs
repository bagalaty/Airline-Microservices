using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Event;

namespace Passenger.Passenger.Events.Domain;

public record PassengerCreatedDomainEvent(string FlightNumber) : IDomainEvent;
