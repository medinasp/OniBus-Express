using OniBusExpress.Application.Abstractions;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.UnitTests.Application;

public sealed class FixedClock : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedClock(DateTimeOffset now) => _now = now;

    public override DateTimeOffset GetUtcNow() => _now;
}

public sealed class FakeTripRepository : ITripRepository
{
    private readonly Trip? _trip;

    public FakeTripRepository(Trip? trip) => _trip = trip;

    public Task<Trip?> GetByIdAsync(Guid tripId, CancellationToken cancellationToken) =>
        Task.FromResult(_trip is not null && _trip.Id == tripId ? _trip : null);
}

public sealed class FakeReservationRepository : IReservationRepository
{
    private readonly Queue<ReservationInsertResult> _outcomes;

    public List<Reservation> Added { get; } = new();

    public FakeReservationRepository(params ReservationInsertResult[] outcomes) =>
        _outcomes = new Queue<ReservationInsertResult>(
            outcomes.Length == 0 ? new[] { ReservationInsertResult.Inserted } : outcomes);

    public Task<ReservationInsertResult> AddAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        var outcome = _outcomes.Count > 1 ? _outcomes.Dequeue() : _outcomes.Peek();
        if (outcome == ReservationInsertResult.Inserted)
        {
            Added.Add(reservation);
        }

        return Task.FromResult(outcome);
    }

    public Task<Reservation?> GetByCodeAsync(ReservationCode code, CancellationToken cancellationToken) =>
        Task.FromResult(Added.FirstOrDefault(r => r.Code == code));

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
