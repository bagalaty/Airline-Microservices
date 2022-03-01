using MediatR;
using Passenger.Passenger.Dtos;
using Passenger.Passenger.Models;

namespace Passenger.Passenger.Features.CompleteRegisterPassenger;

public record CompleteRegisterPassengerCommand(string PassportNumber, PassengerType PassengerType, int Age) : IRequest<PassengerResponseDto>;
