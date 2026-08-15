using OniBusExpress.Application.Trips;

namespace OniBusExpress.Application.Abstractions;

public interface IRouteQueries
{
    Task<IReadOnlyList<RouteDto>> ListAsync(string? origin, string? destination, CancellationToken cancellationToken);
}

public interface ITripQueries
{
    Task<IReadOnlyList<TripSummaryDto>> SearchAsync(TripSearch filter, CancellationToken cancellationToken);

    Task<TripDetailsDto?> GetDetailsAsync(Guid tripId, CancellationToken cancellationToken);
}
