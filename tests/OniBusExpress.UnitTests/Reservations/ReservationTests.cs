using OniBusExpress.Domain.Abstractions;
using OniBusExpress.Domain.Passengers;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.UnitTests.Reservations;

public class ReservationTests
{
    private static readonly DateTimeOffset Agora = new(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);

    private static Trip TripPartindoEm(DateTimeOffset departure, int totalSeats = 40) =>
        new(Guid.NewGuid(), Guid.NewGuid(), departure, departure.AddHours(6), 150m, totalSeats);

    private static (PassengerName Name, Cpf Cpf) Passageiro()
    {
        PassengerName.TryCreate("Maria Silva", out var name);
        Cpf.TryCreate("11144477735", out var cpf);
        return (name!, cpf!);
    }

    [Fact]
    public void Create_ComDadosValidos_CriaReservaConfirmada()
    {
        var trip = TripPartindoEm(Agora.AddDays(1));
        var (name, cpf) = Passageiro();

        var result = Reservation.Create(trip, seatNumber: 12, name, cpf, Agora);

        Assert.True(result.IsSuccess);
        var reserva = result.Value!;
        Assert.Equal(ReservationStatus.Confirmed, reserva.Status);
        Assert.Equal(12, reserva.SeatNumber);
        Assert.Equal(trip.Id, reserva.TripId);
        Assert.Equal(Agora, reserva.CreatedAt);
        Assert.Null(reserva.CancelledAt);
        Assert.NotNull(reserva.Code);
    }

    [Fact]
    public void Create_ComViagemNoPassado_RetornaTripInThePast()
    {
        var trip = TripPartindoEm(Agora.AddMinutes(-1));
        var (name, cpf) = Passageiro();

        var result = Reservation.Create(trip, seatNumber: 12, name, cpf, Agora);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unprocessable, result.Error!.Type);
        Assert.Equal("trip-in-the-past", result.Error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(41)]
    public void Create_ComAssentoForaDoIntervalo_RetornaSeatOutOfRange(int seat)
    {
        var trip = TripPartindoEm(Agora.AddDays(1), totalSeats: 40);
        var (name, cpf) = Passageiro();

        var result = Reservation.Create(trip, seat, name, cpf, Agora);

        Assert.False(result.IsSuccess);
        Assert.Equal("seat-out-of-range", result.Error!.Code);
    }

    [Fact]
    public void Cancel_ConfirmadaDentroDaJanela_MarcaComoCancelada()
    {
        var trip = TripPartindoEm(Agora.AddDays(1));
        var (name, cpf) = Passageiro();
        var reserva = Reservation.Create(trip, 12, name, cpf, Agora).Value!;

        var result = reserva.Cancel(trip, Agora);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reserva.Status);
        Assert.Equal(Agora, reserva.CancelledAt);
    }

    [Fact]
    public void Cancel_JaCancelada_RetornaConflito()
    {
        var trip = TripPartindoEm(Agora.AddDays(1));
        var (name, cpf) = Passageiro();
        var reserva = Reservation.Create(trip, 12, name, cpf, Agora).Value!;
        reserva.Cancel(trip, Agora);

        var result = reserva.Cancel(trip, Agora);

        Assert.False(result.IsSuccess);
        Assert.Equal("reservation-already-cancelled", result.Error!.Code);
    }

    [Fact]
    public void Cancel_ForaDaJanelaDeDuasHoras_RetornaCancellationWindowClosed()
    {
        var trip = TripPartindoEm(Agora.AddHours(1));
        var (name, cpf) = Passageiro();
        var reserva = Reservation.Create(trip, 12, name, cpf, Agora.AddDays(-1)).Value!;

        var result = reserva.Cancel(trip, Agora);

        Assert.False(result.IsSuccess);
        Assert.Equal("cancellation-window-closed", result.Error!.Code);
    }
}
