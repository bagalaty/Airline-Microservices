using BuildingBlocks.Domain;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Refit;
using Reservation.Data;
using Reservation.Flight.Clients;
using Reservation.Flight.Dtos;
using Reservation.Flight.Exceptions;
using Reservation.Passenger.Clients;
using Reservation.Reservation.Dtos;
using Reservation.Reservation.Models.ValueObjects;

namespace Reservation.Reservation.Features.CreateReservation;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ReservationResponseDto>
{
    private readonly IEventProcessor _eventProcessor;
    private readonly ReservationDbContext _reservationDbContext;
    private readonly IFlightServiceClient _flightServiceClient;
    private readonly IPassengerServiceClient _passengerServiceClient;
    private readonly IMapper _mapper;

    public CreateReservationCommandHandler(
        IMapper mapper,
        ReservationDbContext reservationDbContext,
        IFlightServiceClient flightServiceClient,
        IPassengerServiceClient passengerServiceClient)
    {
        _mapper = mapper;
        _reservationDbContext = reservationDbContext;
        _flightServiceClient = flightServiceClient;
        _passengerServiceClient = passengerServiceClient;
    }

    public async Task<ReservationResponseDto> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var flight = await _flightServiceClient.GetById(command.FlightId);
            if (flight is null)
                throw new FlightNotFoundException();

            try
            {
                var passenger = await _passengerServiceClient.GetById(command.PassengerId);

                var emptySeat = (await _flightServiceClient.GetAvailableSeats(command.FlightId))?.First();

                var reservationEntity = Models.Reservation.Create(new PassengerInfo(passenger.Name), new Trip(flight.FlightNumber, flight.AircraftId, flight.DepartureAirportId,
                    flight.ArriveAirportId, flight.FlightDate, flight.Price, command.Description, emptySeat?.SeatNumber));

                await _flightServiceClient.ReserveSeat(new ReserveSeatRequestDto(flight.Id, emptySeat?.SeatNumber));

                var newReservation = await _reservationDbContext.Reservations.AddAsync(reservationEntity, cancellationToken);

                return _mapper.Map<ReservationResponseDto>(newReservation.Entity);
            }
            catch (ValidationApiException ex)
            {
                // todo: fix to better approach in the next
                throw new Exception(ex.Content.Detail);
            }
        }
        catch (ValidationApiException ex)
        {
            throw new Exception(ex.Content.Detail);
        }
    }
}
