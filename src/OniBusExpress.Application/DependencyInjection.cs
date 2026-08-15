using Microsoft.Extensions.DependencyInjection;
using OniBusExpress.Application.Reservations;

namespace OniBusExpress.Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateReservation>();
        services.AddScoped<CancelReservation>();
        services.AddScoped<GetReservation>();
        return services;
    }
}
