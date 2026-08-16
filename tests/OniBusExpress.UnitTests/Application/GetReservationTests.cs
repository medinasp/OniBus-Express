using OniBusExpress.Application.Reservations;
using OniBusExpress.Domain.Abstractions;
using OniBusExpress.Domain.Passengers;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.UnitTests.Application;

public class GetReservationTests
{
    private static readonly DateTimeOffset Agora = new(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);

    private static Reservation ReservaConfirmada()
    {
        var trip = new Trip(Guid.NewGuid(), Guid.NewGuid(), Agora.AddDays(1), Agora.AddDays(1).AddHours(6), 120m, 40);
        PassengerName.TryCreate("Maria Silva", out var name);
        Cpf.TryCreate("11144477735", out var cpf);
        PassengerEmail.TryCreate("maria@exemplo.com", out var email);
        return Reservation.Create(trip, 10, new Passenger(name!, cpf!, email!, null), Agora).Value!;
    }

    [Fact]
    public async Task Handle_Existente_RetornaComCpfMascarado()
    {
        var reserva = ReservaConfirmada();
        var reservas = new FakeReservationRepository();
        reservas.Added.Add(reserva);

        var result = await new GetReservation(reservas).HandleAsync(reserva.Code.Value, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("***.***.**7-35", result.Value!.PassengerCpf);
        Assert.Equal(reserva.Code.Value, result.Value.Code);
    }

    [Fact]
    public async Task Handle_Inexistente_RetornaNotFound()
    {
        var result = await new GetReservation(new FakeReservationRepository())
            .HandleAsync("ABC-12345", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_CodigoMalFormatado_RetornaNotFound()
    {
        var result = await new GetReservation(new FakeReservationRepository())
            .HandleAsync("xyz", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
