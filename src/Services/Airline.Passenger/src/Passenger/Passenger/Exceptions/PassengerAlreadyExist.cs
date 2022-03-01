using BuildingBlocks.Exception;

namespace Passenger.Passenger.Exceptions;

public class PassengerNotExist : BadRequestException
{
    public PassengerNotExist(string code = default) : base("Please register before!")
    {
    }
}
