namespace OniBusExpress.Application.Trips;

public sealed record RouteDto(Guid Id, string Origin, string Destination, TimeSpan EstimatedDuration);

public sealed record TripSearch(string? Origin, string? Destination, DateOnly? Date);

public sealed record TripSummaryDto(
    Guid Id,
    Guid RouteId,
    string Origin,
    string Destination,
    DateTimeOffset DepartureAt,
    DateTimeOffset ArrivalAt,
    decimal Price,
    int TotalSeats,
    int AvailableSeats);

public sealed record SeatDto(int Number, bool Available);

public sealed record TripDetailsDto(
    Guid Id,
    Guid RouteId,
    string Origin,
    string Destination,
    DateTimeOffset DepartureAt,
    DateTimeOffset ArrivalAt,
    decimal Price,
    int TotalSeats,
    int AvailableSeats,
    IReadOnlyList<SeatDto> Seats);
