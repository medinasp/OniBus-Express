using Microsoft.EntityFrameworkCore;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.Infrastructure.Persistence;

public sealed class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _clock;

    public DatabaseSeeder(AppDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.Routes.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = _clock.GetUtcNow();

        var spCampinas = new Route(SeedIds.RouteSpCampinas, "São Paulo", "Campinas");
        var spRio = new Route(SeedIds.RouteSpRio, "São Paulo", "Rio de Janeiro");
        var spSantos = new Route(SeedIds.RouteSpSantos, "São Paulo", "Santos");

        var futura = new Trip(SeedIds.TripFutura, spCampinas.Id, now.AddDays(2), now.AddDays(2).AddHours(1), 45.90m, 40);
        var passada = new Trip(SeedIds.TripPassada, spRio.Id, now.AddDays(-1), now.AddDays(-1).AddHours(6), 120.00m, 44);
        var partindoEmBreve = new Trip(SeedIds.TripPartindoEmBreve, spSantos.Id, now.AddMinutes(90), now.AddMinutes(150), 30.00m, 46);

        _db.Routes.AddRange(spCampinas, spRio, spSantos);
        _db.Trips.AddRange(futura, passada, partindoEmBreve);

        await _db.SaveChangesAsync(cancellationToken);
    }
}

public static class SeedIds
{
    public static readonly Guid RouteSpCampinas = new("a0000000-0000-0000-0000-000000000001");
    public static readonly Guid RouteSpRio = new("a0000000-0000-0000-0000-000000000002");
    public static readonly Guid RouteSpSantos = new("a0000000-0000-0000-0000-000000000003");

    public static readonly Guid TripFutura = new("b0000000-0000-0000-0000-000000000001");
    public static readonly Guid TripPassada = new("b0000000-0000-0000-0000-000000000002");
    public static readonly Guid TripPartindoEmBreve = new("b0000000-0000-0000-0000-000000000003");
}
