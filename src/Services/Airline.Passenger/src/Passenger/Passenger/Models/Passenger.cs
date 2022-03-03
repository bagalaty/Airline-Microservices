using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Model;
using BuildingBlocks.IdsGenerator;

namespace Passenger.Passenger.Models;

public class Passenger : BaseAggregateRoot<long>
{
    public Passenger CompleteRegistrationPassenger(string name, string passportNumber, PassengerType passengerType, int age, long? id = null)
    {
        var passenger = new Passenger
        {
            Name = name,
            PassportNumber = passportNumber,
            PassengerType = passengerType,
            Age = age,
            Id = id ?? SnowFlakIdGenerator.NewId(),
            LastModified = DateTime.Now
        };
        return passenger;
    }


    public static Passenger Create(string name, string passportNumber ,long? id = null)
    {
        var passenger = new Passenger {Name = name, PassportNumber = passportNumber, Id = id ?? SnowFlakIdGenerator.NewId()};
        return passenger;
    }


    public string PassportNumber { get; private set; }
    public string Name { get; private set; }
    public PassengerType PassengerType { get; private set; }
    public int Age { get; private set; }
}
