using Microsoft.EntityFrameworkCore;
using OniBusExpress.Infrastructure.Persistence;
using OniBusExpress.IntegrationTests.Infrastructure;

namespace OniBusExpress.IntegrationTests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class DatabaseSeederTests : IntegrationTestBase
{
    public DatabaseSeederTests(PostgresFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task SeedAsync_EmBaseSemDados_CriaRotasEViagensDosTresCasos()
    {
        await using var db = Fixture.CreateContext();
        await new DatabaseSeeder(db, TimeProvider.System).SeedAsync();

        var now = DateTimeOffset.UtcNow;
        var trips = await db.Trips.AsNoTracking().ToListAsync();

        Assert.True(await db.Routes.AnyAsync());
        Assert.Contains(trips, t => t.DepartureAt > now.AddHours(2));
        Assert.Contains(trips, t => t.DepartureAt <= now);
        Assert.Contains(trips, t => t.DepartureAt > now && t.DepartureAt <= now.AddHours(2));
    }

    [Fact]
    public async Task SeedAsync_ExecutadoDuasVezes_NaoDuplica()
    {
        await using var db = Fixture.CreateContext();
        var seeder = new DatabaseSeeder(db, TimeProvider.System);

        await seeder.SeedAsync();
        var rotasApos1 = await db.Routes.CountAsync();
        await seeder.SeedAsync();
        var rotasApos2 = await db.Routes.CountAsync();

        Assert.Equal(rotasApos1, rotasApos2);
    }
}
