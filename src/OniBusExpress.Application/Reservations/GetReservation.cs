using OniBusExpress.Application.Abstractions;
using OniBusExpress.Domain.Abstractions;
using OniBusExpress.Domain.Reservations;

namespace OniBusExpress.Application.Reservations;

public sealed class GetReservation
{
    private readonly IReservationRepository _reservations;

    public GetReservation(IReservationRepository reservations) => _reservations = reservations;

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

        return ReservationResponse.From(reservation);
    }
}
