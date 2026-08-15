using OniBusExpress.Application.Trips;

namespace OniBusExpress.Application.Abstractions;

public interface IRouteQueries
{
    Task<IReadOnlyList<RouteDto>> ListAsync(string? origin, string? destination, Pagination pagination, CancellationToken cancellationToken);
}

public interface ITripQueries
{
    Task<IReadOnlyList<TripSummaryDto>> SearchAsync(TripSearch filter, Pagination pagination, CancellationToken cancellationToken);

    Task<TripDetailsDto?> GetDetailsAsync(Guid tripId, CancellationToken cancellationToken);
}
