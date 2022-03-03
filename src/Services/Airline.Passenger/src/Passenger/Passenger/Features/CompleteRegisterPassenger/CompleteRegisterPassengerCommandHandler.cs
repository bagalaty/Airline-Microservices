using BuildingBlocks.Domain;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Passenger.Data;
using Passenger.Passenger.Dtos;
using Passenger.Passenger.Exceptions;

namespace Passenger.Passenger.Features.CompleteRegisterPassenger;

public class CompleteRegisterPassengerCommandHandler : IRequestHandler<CompleteRegisterPassengerCommand, PassengerResponseDto>
{
    private readonly IMapper _mapper;
    private readonly PassengerDbContext _passengerDbContext;

    public CompleteRegisterPassengerCommandHandler(IMapper mapper, PassengerDbContext passengerDbContext)
    {
        _mapper = mapper;
        _passengerDbContext = passengerDbContext;
    }

    public async Task<PassengerResponseDto> Handle(CompleteRegisterPassengerCommand command, CancellationToken cancellationToken)
    {
        var passenger = await _passengerDbContext.Passengers.AsNoTracking().SingleOrDefaultAsync(
            x => x.PassportNumber == command.PassportNumber,
            cancellationToken);

        if (passenger is null)
            throw new PassengerNotExist();

        var passengerEntity = passenger.CompleteRegistrationPassenger(passenger.Name, passenger.PassportNumber, command.PassengerType, command.Age, passenger.Id);

        var updatePassenger = _passengerDbContext.Passengers.Update(passengerEntity);

        return _mapper.Map<PassengerResponseDto>(updatePassenger.Entity);
    }
}
