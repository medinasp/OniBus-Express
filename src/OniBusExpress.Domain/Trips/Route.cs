namespace OniBusExpress.Domain.Trips;

public sealed class Route
{
    public Guid Id { get; private set; }
    public string Origin { get; private set; }
    public string Destination { get; private set; }

    public Route(Guid id, string origin, string destination)
    {
        Id = id;
        Origin = origin;
        Destination = destination;
    }
}
