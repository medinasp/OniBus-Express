using OniBusExpress.Domain.Trips;

namespace OniBusExpress.Application.Abstractions;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(Guid tripId, CancellationToken cancellationToken);
}
