using BuildingBlocks.Exception;

namespace Passenger.Passenger.Exceptions;

public class PassengerNotFoundException: NotFoundException
{
    public PassengerNotFoundException(string code = default) : base("Passenger not found!")
    {
    }
}
