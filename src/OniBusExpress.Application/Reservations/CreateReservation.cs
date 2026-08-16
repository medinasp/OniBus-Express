using OniBusExpress.Application.Abstractions;
using OniBusExpress.Domain.Abstractions;
using OniBusExpress.Domain.Passengers;
using OniBusExpress.Domain.Reservations;

namespace OniBusExpress.Application.Reservations;

public sealed record CreateReservationCommand(
    Guid TripId,
    int SeatNumber,
    string? PassengerName,
    string? PassengerCpf,
    string? PassengerEmail,
    DateOnly? PassengerDateOfBirth);

public sealed record ReservationResponse(
    string Code,
    Guid TripId,
    int SeatNumber,
    string PassengerName,
    string PassengerCpf,
    string PassengerEmail,
    DateOnly? PassengerDateOfBirth,
    string Status,
    DateTimeOffset CreatedAt)
{
    public static ReservationResponse From(Reservation reservation) =>
        new(
            reservation.Code.Value,
            reservation.TripId,
            reservation.SeatNumber,
            reservation.Passenger.Name.Value,
            reservation.Passenger.Cpf.Masked,
            reservation.Passenger.Email.Value,
            reservation.Passenger.DateOfBirth,
            reservation.Status.ToString(),
            reservation.CreatedAt);
}

public sealed class CreateReservation
{
    private const int MaxCodeGenerationAttempts = 5;

    private readonly ITripRepository _trips;
    private readonly IReservationRepository _reservations;
    private readonly TimeProvider _clock;

    public CreateReservation(ITripRepository trips, IReservationRepository reservations, TimeProvider clock)
    {
        _trips = trips;
        _reservations = reservations;
        _clock = clock;
    }

    public async Task<Result<ReservationResponse>> HandleAsync(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        if (!Cpf.TryCreate(command.PassengerCpf, out var cpf))
        {
            return DomainError.Validation("CPF do passageiro é inválido.");
        }

        if (!PassengerName.TryCreate(command.PassengerName, out var name))
        {
            return DomainError.Validation("Nome do passageiro é obrigatório.");
        }

        if (!PassengerEmail.TryCreate(command.PassengerEmail, out var email))
        {
            return DomainError.Validation("E-mail do passageiro é inválido.");
        }

        var trip = await _trips.GetByIdAsync(command.TripId, cancellationToken);
        if (trip is null)
        {
            return DomainError.NotFound("Viagem não encontrada.");
        }

        var now = _clock.GetUtcNow();

        var passenger = new Passenger(name!, cpf!, email!, command.PassengerDateOfBirth);

        for (var attempt = 0; attempt < MaxCodeGenerationAttempts; attempt++)
        {
            var creation = Reservation.Create(trip, command.SeatNumber, passenger, now);
            if (!creation.IsSuccess)
            {
                return creation.Error!;
            }

            var reservation = creation.Value!;
            var outcome = await _reservations.AddAsync(reservation, cancellationToken);

            switch (outcome)
            {
                case ReservationInsertResult.Inserted:
                    return ReservationResponse.From(reservation);
                case ReservationInsertResult.SeatAlreadyTaken:
                    return DomainError.SeatAlreadyTaken();
                case ReservationInsertResult.CodeCollision:
                    continue;
            }
        }

        throw new InvalidOperationException("Não foi possível gerar um código de reserva único.");
    }
}
