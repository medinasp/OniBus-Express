using OniBusExpress.Application.Reservations;
using OniBusExpress.Domain.Abstractions;
using OniBusExpress.Domain.Passengers;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.UnitTests.Application;

public class CancelReservationTests
{
    private static readonly DateTimeOffset Agora = new(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);

    private static Reservation ReservaConfirmada(Trip trip)
    {
        PassengerName.TryCreate("Maria Silva", out var name);
        Cpf.TryCreate("11144477735", out var cpf);
        PassengerEmail.TryCreate("maria@exemplo.com", out var email);
        return Reservation.Create(trip, 10, new Passenger(name!, cpf!, email!, null), Agora.AddDays(-2)).Value!;
    }

    private CancelReservation UseCase(Trip? trip, FakeReservationRepository reservas) =>
        new(new FakeTripRepository(trip), reservas, new FixedClock(Agora));

    [Fact]
    public async Task Handle_ConfirmadaDentroDaJanela_CancelaComSucesso()
    {
        var trip = new Trip(Guid.NewGuid(), Guid.NewGuid(), Agora.AddDays(1), Agora.AddDays(1).AddHours(6), 120m, 40);
        var reserva = ReservaConfirmada(trip);
        var reservas = new FakeReservationRepository();
        reservas.Added.Add(reserva);

        var result = await UseCase(trip, reservas).HandleAsync(reserva.Code.Value, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Cancelled", result.Value!.Status);
        Assert.Equal(ReservationStatus.Cancelled, reserva.Status);
    }

    [Fact]
    public async Task Handle_CodigoInexistente_RetornaNotFound()
    {
        var result = await UseCase(null, new FakeReservationRepository())
            .HandleAsync("ABC-12345", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_CodigoMalFormatado_RetornaNotFound()
    {
        var result = await UseCase(null, new FakeReservationRepository())
            .HandleAsync("formato-invalido", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_JaCancelada_RetornaConflito()
    {
        var trip = new Trip(Guid.NewGuid(), Guid.NewGuid(), Agora.AddDays(1), Agora.AddDays(1).AddHours(6), 120m, 40);
        var reserva = ReservaConfirmada(trip);
        reserva.Cancel(trip, Agora);
        var reservas = new FakeReservationRepository();
        reservas.Added.Add(reserva);

        var result = await UseCase(trip, reservas).HandleAsync(reserva.Code.Value, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("reservation-already-cancelled", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ForaDaJanelaDeDuasHoras_RetornaCancellationWindowClosed()
    {
        var trip = new Trip(Guid.NewGuid(), Guid.NewGuid(), Agora.AddHours(1), Agora.AddHours(7), 120m, 40);
        var reserva = ReservaConfirmada(trip);
        var reservas = new FakeReservationRepository();
        reservas.Added.Add(reserva);

        var result = await UseCase(trip, reservas).HandleAsync(reserva.Code.Value, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("cancellation-window-closed", result.Error!.Code);
    }
}
