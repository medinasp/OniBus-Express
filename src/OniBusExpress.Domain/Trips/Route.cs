namespace OniBusExpress.Domain.Trips;

public sealed class Route
{
    public Guid Id { get; private set; }
    public string Origin { get; private set; }
    public string Destination { get; private set; }
    public TimeSpan EstimatedDuration { get; private set; }

    public Route(Guid id, string origin, string destination, TimeSpan estimatedDuration)
    {
        Id = id;
        Origin = origin;
        Destination = destination;
        EstimatedDuration = estimatedDuration;
    }
}
