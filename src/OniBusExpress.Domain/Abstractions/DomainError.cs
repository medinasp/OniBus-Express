namespace OniBusExpress.Domain.Abstractions;

public sealed record DomainError(string Code, ErrorType Type, string Description)
{
    public static DomainError Validation(string description) =>
        new("validation-error", ErrorType.Validation, description);

    public static DomainError NotFound(string description) =>
        new("resource-not-found", ErrorType.NotFound, description);

    public static DomainError SeatAlreadyTaken() =>
        new("seat-already-taken", ErrorType.Conflict, "O assento já está reservado para esta viagem.");

    public static DomainError ReservationAlreadyCancelled() =>
        new("reservation-already-cancelled", ErrorType.Conflict, "A reserva já está cancelada.");

    public static DomainError TripInThePast() =>
        new("trip-in-the-past", ErrorType.Unprocessable, "A viagem já partiu.");

    public static DomainError SeatOutOfRange(int seatNumber, int totalSeats) =>
        new("seat-out-of-range", ErrorType.Unprocessable, $"O assento {seatNumber} não existe nesta viagem (1 a {totalSeats}).");

    public static DomainError CancellationWindowClosed() =>
        new("cancellation-window-closed", ErrorType.Unprocessable, "O cancelamento só é permitido até 2 horas antes da partida.");
}
