using Microsoft.EntityFrameworkCore;
using Npgsql;
using OniBusExpress.Application.Abstractions;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Infrastructure.Persistence;

namespace OniBusExpress.Infrastructure.Reservations;

public sealed class ReservationRepository : IReservationRepository
{
    private const string ActiveSeatConstraint = "ux_reservation_active_seat";
    private const string CodeConstraint = "ux_reservation_code";

    private readonly AppDbContext _db;

    public ReservationRepository(AppDbContext db) => _db = db;

    public async Task<ReservationInsertResult> AddAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        _db.Reservations.Add(reservation);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return ReservationInsertResult.Inserted;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            _db.Entry(reservation).State = EntityState.Detached;

            if (pg.ConstraintName == ActiveSeatConstraint)
            {
                return ReservationInsertResult.SeatAlreadyTaken;
            }

            if (pg.ConstraintName == CodeConstraint)
            {
                return ReservationInsertResult.CodeCollision;
            }

            throw;
        }
    }

    public Task<Reservation?> GetByCodeAsync(ReservationCode code, CancellationToken cancellationToken) =>
        _db.Reservations.SingleOrDefaultAsync(r => r.Code == code, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
