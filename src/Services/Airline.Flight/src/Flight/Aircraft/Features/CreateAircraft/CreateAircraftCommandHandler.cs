using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using Flight.Aircraft.Dtos;
using Flight.Aircraft.Exceptions;
using Flight.Data;
using Flight.Flight.Dtos;
using Flight.Flight.Exceptions;
using Flight.Flight.Models;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flight.Aircraft.Features.CreateAircraft;

public class CreateAircraftCommandHandler : IRequestHandler<CreateAircraftCommand, AircraftResponseDto>
{
    private readonly FlightDbContext _flightDbContext;
    private readonly IMapper _mapper;

    public CreateAircraftCommandHandler(IMapper mapper, FlightDbContext flightDbContext)
    {
        _mapper = mapper;
        _flightDbContext = flightDbContext;
    }

    public async Task<AircraftResponseDto> Handle(CreateAircraftCommand command, CancellationToken cancellationToken)
    {
        var aircraft = await _flightDbContext.Aircraft.SingleOrDefaultAsync(x => x.Model == command.Model, cancellationToken);

        if (aircraft is not null)
            throw new AircraftAlreadyExistException();

        var aircraftEntity = Models.Aircraft.Create(command.Name, command.Model, command.ManufacturingYear);

        var newAircraft = await _flightDbContext.Aircraft.AddAsync(aircraftEntity, cancellationToken);

        await _flightDbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AircraftResponseDto>(newAircraft.Entity);
    }
}
