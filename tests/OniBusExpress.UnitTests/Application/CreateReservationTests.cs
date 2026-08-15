using OniBusExpress.Application.Abstractions;
using OniBusExpress.Application.Reservations;
using OniBusExpress.Domain.Abstractions;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.UnitTests.Application;

public class CreateReservationTests
{
    private static readonly DateTimeOffset Agora = new(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);

    private static Trip TripFutura(int totalSeats = 40) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Agora.AddDays(1), Agora.AddDays(1).AddHours(6), 120m, totalSeats);

    private static CreateReservationCommand Comando(Guid tripId, int seat = 10) =>
        new(tripId, seat, "Maria Silva", "111.444.777-35");

    private CreateReservation UseCase(ITripRepository trips, IReservationRepository reservas) =>
        new(trips, reservas, new FixedClock(Agora));

    [Fact]
    public async Task Handle_DadosValidos_CriaReservaConfirmadaComCpfMascarado()
    {
        var trip = TripFutura();
        var useCase = UseCase(new FakeTripRepository(trip), new FakeReservationRepository());

        var result = await useCase.HandleAsync(Comando(trip.Id, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Confirmed", result.Value!.Status);
        Assert.Equal(10, result.Value.SeatNumber);
        Assert.Equal("***.***.**7-35", result.Value.PassengerCpf);
    }

    [Fact]
    public async Task Handle_CpfInvalido_RetornaValidation()
    {
        var trip = TripFutura();
        var useCase = UseCase(new FakeTripRepository(trip), new FakeReservationRepository());
        var comando = new CreateReservationCommand(trip.Id, 10, "Maria Silva", "11111111111");

        var result = await useCase.HandleAsync(comando, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_NomeVazio_RetornaValidation()
    {
        var trip = TripFutura();
        var useCase = UseCase(new FakeTripRepository(trip), new FakeReservationRepository());
        var comando = new CreateReservationCommand(trip.Id, 10, "   ", "111.444.777-35");

        var result = await useCase.HandleAsync(comando, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_ViagemInexistente_RetornaNotFound()
    {
        var useCase = UseCase(new FakeTripRepository(null), new FakeReservationRepository());

        var result = await useCase.HandleAsync(Comando(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_AssentoForaDoIntervalo_RetornaSeatOutOfRange()
    {
        var trip = TripFutura(totalSeats: 40);
        var useCase = UseCase(new FakeTripRepository(trip), new FakeReservationRepository());

        var result = await useCase.HandleAsync(Comando(trip.Id, seat: 99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("seat-out-of-range", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_AssentoJaReservado_RetornaSeatAlreadyTaken()
    {
        var trip = TripFutura();
        var reservas = new FakeReservationRepository(ReservationInsertResult.SeatAlreadyTaken);
        var useCase = UseCase(new FakeTripRepository(trip), reservas);

        var result = await useCase.HandleAsync(Comando(trip.Id, 10), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("seat-already-taken", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ColisaoDeCodigo_RegeneraETemSucesso()
    {
        var trip = TripFutura();
        var reservas = new FakeReservationRepository(
            ReservationInsertResult.CodeCollision,
            ReservationInsertResult.Inserted);
        var useCase = UseCase(new FakeTripRepository(trip), reservas);

        var result = await useCase.HandleAsync(Comando(trip.Id, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(reservas.Added);
    }

    [Fact]
    public async Task Handle_ColisaoDeCodigoPersistente_LancaAposEsgotarTentativas()
    {
        var trip = TripFutura();
        var reservas = new FakeReservationRepository(ReservationInsertResult.CodeCollision);
        var useCase = UseCase(new FakeTripRepository(trip), reservas);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.HandleAsync(Comando(trip.Id, 10), CancellationToken.None));
    }
}
