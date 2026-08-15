using OniBusExpress.Application.Abstractions;
using OniBusExpress.Domain.Abstractions;
using OniBusExpress.Domain.Reservations;

namespace OniBusExpress.Application.Reservations;

public sealed class CancelReservation
{
    private readonly ITripRepository _trips;
    private readonly IReservationRepository _reservations;
    private readonly TimeProvider _clock;

    public CancelReservation(ITripRepository trips, IReservationRepository reservations, TimeProvider clock)
    {
        _trips = trips;
        _reservations = reservations;
        _clock = clock;
    }

    public async Task<Result<ReservationResponse>> HandleAsync(string code, CancellationToken cancellationToken)
    {
        if (!ReservationCode.TryParse(code, out var reservationCode))
        {
            return DomainError.NotFound("Reserva não encontrada.");
        }

        var reservation = await _reservations.GetByCodeAsync(reservationCode!, cancellationToken);
        if (reservation is null)
        {
            return DomainError.NotFound("Reserva não encontrada.");
        }

        var trip = await _trips.GetByIdAsync(reservation.TripId, cancellationToken);
        if (trip is null)
        {
            return DomainError.NotFound("Viagem não encontrada.");
        }

        var cancellation = reservation.Cancel(trip, _clock.GetUtcNow());
        if (!cancellation.IsSuccess)
        {
            return cancellation.Error!;
        }

        await _reservations.SaveChangesAsync(cancellationToken);
        return ReservationResponse.From(reservation);
    }
}
