using Refit;
using Reservation.Flight.Dtos;

namespace Reservation.Flight.Clients;

public interface IFlightServiceClient
{
    [Get("/api/v1/flight/{id}")]
    [Headers("Authorization: Bearer")]
    Task<FlightResponseDto> GetById(long id);

    [Get("/api/v1/flight/get-available-seats/{flightId}")]
    [Headers("Authorization: Bearer")]
    Task<IEnumerable<SeatResponseDto>> GetAvailableSeats(long flightId);

    [Post("/api/v1/flight/reserve-seat")]
    [Headers("Authorization: Bearer")]
    Task<FlightResponseDto> ReserveSeat(ReserveSeatRequestDto request);
}
