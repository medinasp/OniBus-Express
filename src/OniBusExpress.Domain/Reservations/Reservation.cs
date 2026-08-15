using OniBusExpress.Domain.Abstractions;
using OniBusExpress.Domain.Passengers;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.Domain.Reservations;

public sealed class Reservation
{
    public Guid Id { get; private set; }
    public ReservationCode Code { get; private set; }
    public Guid TripId { get; private set; }
    public int SeatNumber { get; private set; }
    public PassengerName PassengerName { get; private set; }
    public Cpf PassengerCpf { get; private set; }
    public PassengerEmail PassengerEmail { get; private set; }
    public DateOnly? PassengerDateOfBirth { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    private Reservation(
        Guid id,
        ReservationCode code,
        Guid tripId,
        int seatNumber,
        PassengerName passengerName,
        Cpf passengerCpf,
        PassengerEmail passengerEmail,
        DateOnly? passengerDateOfBirth,
        ReservationStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? cancelledAt)
    {
        Id = id;
        Code = code;
        TripId = tripId;
        SeatNumber = seatNumber;
        PassengerName = passengerName;
        PassengerCpf = passengerCpf;
        PassengerEmail = passengerEmail;
        PassengerDateOfBirth = passengerDateOfBirth;
        Status = status;
        CreatedAt = createdAt;
        CancelledAt = cancelledAt;
    }

    public static Result<Reservation> Create(
        Trip trip,
        int seatNumber,
        PassengerName passengerName,
        Cpf passengerCpf,
        PassengerEmail passengerEmail,
        DateOnly? passengerDateOfBirth,
        DateTimeOffset now)
    {
        if (trip.HasDeparted(now))
        {
            return DomainError.TripInThePast();
        }

        if (!trip.IsSeatWithinRange(seatNumber))
        {
            return DomainError.SeatOutOfRange(seatNumber, trip.TotalSeats);
        }

        return new Reservation(
            Guid.NewGuid(),
            ReservationCode.Generate(),
            trip.Id,
            seatNumber,
            passengerName,
            passengerCpf,
            passengerEmail,
            passengerDateOfBirth,
            ReservationStatus.Confirmed,
            now,
            null);
    }

    internal static Reservation Rehydrate(
        Guid id,
        ReservationCode code,
        Guid tripId,
        int seatNumber,
        PassengerName passengerName,
        Cpf passengerCpf,
        PassengerEmail passengerEmail,
        DateOnly? passengerDateOfBirth,
        ReservationStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? cancelledAt) =>
        new(id, code, tripId, seatNumber, passengerName, passengerCpf, passengerEmail, passengerDateOfBirth, status, createdAt, cancelledAt);

    public Result Cancel(Trip trip, DateTimeOffset now)
    {
        if (Status == ReservationStatus.Cancelled)
        {
            return DomainError.ReservationAlreadyCancelled();
        }

        if (!trip.IsCancellationAllowed(now))
        {
            return DomainError.CancellationWindowClosed();
        }

        Status = ReservationStatus.Cancelled;
        CancelledAt = now;
        return Result.Success();
    }
}
