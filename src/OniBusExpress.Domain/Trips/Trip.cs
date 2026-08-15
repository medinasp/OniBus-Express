namespace OniBusExpress.Domain.Trips;

public sealed class Trip
{
    private static readonly TimeSpan CancellationWindow = TimeSpan.FromHours(2);

    public Guid Id { get; private set; }
    public Guid RouteId { get; private set; }
    public DateTimeOffset DepartureAt { get; private set; }
    public DateTimeOffset ArrivalAt { get; private set; }
    public decimal Price { get; private set; }
    public int TotalSeats { get; private set; }

    public Trip(Guid id, Guid routeId, DateTimeOffset departureAt, DateTimeOffset arrivalAt, decimal price, int totalSeats)
    {
        Id = id;
        RouteId = routeId;
        DepartureAt = departureAt;
        ArrivalAt = arrivalAt;
        Price = price;
        TotalSeats = totalSeats;
    }

    public DateTimeOffset CancellationDeadline => DepartureAt - CancellationWindow;

    public bool HasDeparted(DateTimeOffset now) => DepartureAt <= now;

    public bool IsSeatWithinRange(int seatNumber) => seatNumber >= 1 && seatNumber <= TotalSeats;

    public bool IsCancellationAllowed(DateTimeOffset now) => now <= CancellationDeadline;
}
