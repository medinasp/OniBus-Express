using Microsoft.EntityFrameworkCore;
using OniBusExpress.Infrastructure.Persistence;

namespace OniBusExpress.Api.Startup;

public static class DatabaseStartup
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        await db.Database.MigrateAsync();
        await new DatabaseSeeder(db, clock).SeedAsync();
    }
}
