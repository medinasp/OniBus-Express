using Microsoft.EntityFrameworkCore;
using OniBusExpress.Application.Abstractions;
using OniBusExpress.Application.Trips;
using OniBusExpress.Infrastructure.Persistence;

namespace OniBusExpress.Infrastructure.Trips;

public sealed class RouteQueries : IRouteQueries
{
    private readonly AppDbContext _db;

    public RouteQueries(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<RouteDto>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Routes.AsNoTracking()
            .OrderBy(r => r.Origin)
            .ThenBy(r => r.Destination)
            .Select(r => new RouteDto(r.Id, r.Origin, r.Destination))
            .ToListAsync(cancellationToken);
}
