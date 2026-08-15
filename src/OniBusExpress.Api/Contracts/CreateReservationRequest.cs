namespace OniBusExpress.Api.Contracts;

public sealed record CreateReservationRequest(Guid TripId, int SeatNumber, PassengerRequest? Passenger);

public sealed record PassengerRequest(string? Name, string? Cpf);
