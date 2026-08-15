using Microsoft.EntityFrameworkCore;
using OniBusExpress.Application.Abstractions;
using OniBusExpress.Domain.Trips;
using OniBusExpress.Infrastructure.Persistence;

namespace OniBusExpress.Infrastructure.Trips;

public sealed class TripRepository : ITripRepository
{
    private readonly AppDbContext _db;

    public TripRepository(AppDbContext db) => _db = db;

    public Task<Trip?> GetByIdAsync(Guid tripId, CancellationToken cancellationToken) =>
        _db.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tripId, cancellationToken);
}
