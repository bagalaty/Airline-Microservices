using Refit;
using Reservation.Passenger.Dtos;

namespace Reservation.Passenger.Clients;

public interface IPassengerServiceClient
{
    [Get("/api/v1/passenger/{id}")]
    [Headers("Authorization: Bearer")]
    Task<PassengerResponseDto> GetById(long id);
}
