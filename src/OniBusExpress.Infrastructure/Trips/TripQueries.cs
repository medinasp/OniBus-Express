using Microsoft.EntityFrameworkCore;
using OniBusExpress.Application.Abstractions;
using OniBusExpress.Application.Trips;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Infrastructure.Persistence;

namespace OniBusExpress.Infrastructure.Trips;

public sealed class TripQueries : ITripQueries
{
    private readonly AppDbContext _db;

    public TripQueries(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<TripSummaryDto>> SearchAsync(TripSearch filter, CancellationToken cancellationToken)
    {
        var query =
            from t in _db.Trips.AsNoTracking()
            join r in _db.Routes.AsNoTracking() on t.RouteId equals r.Id
            select new { Trip = t, r.Origin, r.Destination };

        if (!string.IsNullOrWhiteSpace(filter.Origin))
        {
            var origin = filter.Origin.Trim().ToLower();
            query = query.Where(x => x.Origin.ToLower() == origin);
        }

        if (!string.IsNullOrWhiteSpace(filter.Destination))
        {
            var destination = filter.Destination.Trim().ToLower();
            query = query.Where(x => x.Destination.ToLower() == destination);
        }

        if (filter.Date is { } date)
        {
            var start = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var end = start.AddDays(1);
            query = query.Where(x => x.Trip.DepartureAt >= start && x.Trip.DepartureAt < end);
        }

        return await query
            .OrderBy(x => x.Trip.DepartureAt)
            .Select(x => new TripSummaryDto(
                x.Trip.Id,
                x.Trip.RouteId,
                x.Origin,
                x.Destination,
                x.Trip.DepartureAt,
                x.Trip.ArrivalAt,
                x.Trip.Price,
                x.Trip.TotalSeats,
                x.Trip.TotalSeats - _db.Reservations.Count(res => res.TripId == x.Trip.Id && res.Status == ReservationStatus.Confirmed)))
            .ToListAsync(cancellationToken);
    }

    public async Task<TripDetailsDto?> GetDetailsAsync(Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await (
            from t in _db.Trips.AsNoTracking()
            join r in _db.Routes.AsNoTracking() on t.RouteId equals r.Id
            where t.Id == tripId
            select new
            {
                t.Id,
                t.RouteId,
                r.Origin,
                r.Destination,
                t.DepartureAt,
                t.ArrivalAt,
                t.Price,
                t.TotalSeats
            }).FirstOrDefaultAsync(cancellationToken);

        if (trip is null)
        {
            return null;
        }

        var takenSeats = await _db.Reservations.AsNoTracking()
            .Where(res => res.TripId == tripId && res.Status == ReservationStatus.Confirmed)
            .Select(res => res.SeatNumber)
            .ToListAsync(cancellationToken);

        var taken = takenSeats.ToHashSet();
        var seats = Enumerable.Range(1, trip.TotalSeats)
            .Select(number => new SeatDto(number, !taken.Contains(number)))
            .ToList();

        return new TripDetailsDto(
            trip.Id,
            trip.RouteId,
            trip.Origin,
            trip.Destination,
            trip.DepartureAt,
            trip.ArrivalAt,
            trip.Price,
            trip.TotalSeats,
            trip.TotalSeats - taken.Count,
            seats);
    }
}
