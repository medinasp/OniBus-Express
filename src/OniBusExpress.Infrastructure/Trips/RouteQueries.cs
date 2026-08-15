using Microsoft.EntityFrameworkCore;
using OniBusExpress.Application.Abstractions;
using OniBusExpress.Application.Trips;
using OniBusExpress.Infrastructure.Persistence;

namespace OniBusExpress.Infrastructure.Trips;

public sealed class RouteQueries : IRouteQueries
{
    private readonly AppDbContext _db;

    public RouteQueries(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<RouteDto>> ListAsync(string? origin, string? destination, CancellationToken cancellationToken)
    {
        var query = _db.Routes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(origin))
        {
            var value = origin.Trim().ToLower();
            query = query.Where(r => r.Origin.ToLower() == value);
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            var value = destination.Trim().ToLower();
            query = query.Where(r => r.Destination.ToLower() == value);
        }

        return await query
            .OrderBy(r => r.Origin)
            .ThenBy(r => r.Destination)
            .Select(r => new RouteDto(r.Id, r.Origin, r.Destination))
            .ToListAsync(cancellationToken);
    }
}
