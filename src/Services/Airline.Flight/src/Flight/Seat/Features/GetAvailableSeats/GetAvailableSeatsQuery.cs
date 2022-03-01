using System.Collections.Generic;
using Flight.Seat.Dtos;
using MediatR;

namespace Flight.Seat.Features.GetAvailableSeats;

public class GetAvailableSeatsQuery : IRequest<IEnumerable<SeatResponseDto>>
{
    // public GetAvailableSeatsQuery(long FlightId)
    // {
    //     this.FlightId = FlightId;
    // }

    public long FlightId { get; set; }
}
