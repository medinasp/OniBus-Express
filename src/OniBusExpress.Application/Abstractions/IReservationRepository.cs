using OniBusExpress.Domain.Reservations;

namespace OniBusExpress.Application.Abstractions;

public enum ReservationInsertResult
{
    Inserted,
    SeatAlreadyTaken,
    CodeCollision
}

public interface IReservationRepository
{
    Task<ReservationInsertResult> AddAsync(Reservation reservation, CancellationToken cancellationToken);

    Task<Reservation?> GetByCodeAsync(ReservationCode code, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
