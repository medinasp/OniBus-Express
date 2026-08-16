namespace OniBusExpress.Domain.Passengers;

public sealed record Passenger(PassengerName Name, Cpf Cpf, PassengerEmail Email, DateOnly? DateOfBirth);
