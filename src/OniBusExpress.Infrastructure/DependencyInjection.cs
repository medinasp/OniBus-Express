using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OniBusExpress.Application.Abstractions;
using OniBusExpress.Infrastructure.Persistence;
using OniBusExpress.Infrastructure.Reservations;
using OniBusExpress.Infrastructure.Trips;

namespace OniBusExpress.Infrastructure;

public static class InfrastructureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IRouteQueries, RouteQueries>();
        services.AddScoped<ITripQueries, TripQueries>();

        return services;
    }
}
