using OniBusExpress.Application.Abstractions;
using OniBusExpress.Domain.Passengers;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Domain.Trips;
using OniBusExpress.Infrastructure.Reservations;
using OniBusExpress.IntegrationTests.Infrastructure;

namespace OniBusExpress.IntegrationTests.Reservations;

[Collection(DatabaseCollection.Name)]
public sealed class ReservationRepositoryTests : IntegrationTestBase
{
    public ReservationRepositoryTests(PostgresFixture fixture) : base(fixture)
    {
    }

    private async Task<Trip> SeedTripAsync(int totalSeats = 40)
    {
        var partida = DateTimeOffset.UtcNow.AddDays(1);
        var route = new Route(Guid.NewGuid(), "São Paulo", "Rio de Janeiro");
        var trip = new Trip(Guid.NewGuid(), route.Id, partida, partida.AddHours(6), 120m, totalSeats);

        await using var db = Fixture.CreateContext();
        db.Add(route);
        db.Add(trip);
        await db.SaveChangesAsync();
        return trip;
    }

    private static Reservation NovaReserva(Trip trip, int seat)
    {
        PassengerName.TryCreate("João Souza", out var name);
        Cpf.TryCreate("11144477735", out var cpf);
        return Reservation.Create(trip, seat, name!, cpf!, DateTimeOffset.UtcNow).Value!;
    }

    [Fact]
    public async Task AddAsync_AssentoLivre_InsereReserva()
    {
        var trip = await SeedTripAsync();

        await using var db = Fixture.CreateContext();
        var repo = new ReservationRepository(db);

        var resultado = await repo.AddAsync(NovaReserva(trip, 10), CancellationToken.None);

        Assert.Equal(ReservationInsertResult.Inserted, resultado);
    }

    [Fact]
    public async Task AddAsync_AssentoJaConfirmado_RetornaSeatAlreadyTaken()
    {
        var trip = await SeedTripAsync();

        await using (var db = Fixture.CreateContext())
        {
            await new ReservationRepository(db).AddAsync(NovaReserva(trip, 20), CancellationToken.None);
        }

        await using (var db = Fixture.CreateContext())
        {
            var resultado = await new ReservationRepository(db).AddAsync(NovaReserva(trip, 20), CancellationToken.None);
            Assert.Equal(ReservationInsertResult.SeatAlreadyTaken, resultado);
        }
    }

    [Fact]
    public async Task AddAsync_RequisicoesParalelasNoMesmoAssento_ApenasUmaVence()
    {
        var trip = await SeedTripAsync();
        const int paralelas = 8;
        const int assento = 5;

        var tarefas = Enumerable.Range(0, paralelas).Select(async _ =>
        {
            await using var db = Fixture.CreateContext();
            return await new ReservationRepository(db).AddAsync(NovaReserva(trip, assento), CancellationToken.None);
        });

        var resultados = await Task.WhenAll(tarefas);

        Assert.Equal(1, resultados.Count(r => r == ReservationInsertResult.Inserted));
        Assert.Equal(paralelas - 1, resultados.Count(r => r == ReservationInsertResult.SeatAlreadyTaken));
    }

    [Fact]
    public async Task AddAsync_AposCancelamento_LiberaAssentoParaNovaReserva()
    {
        var trip = await SeedTripAsync();
        var reserva = NovaReserva(trip, 15);

        await using (var db = Fixture.CreateContext())
        {
            await new ReservationRepository(db).AddAsync(reserva, CancellationToken.None);
        }

        await using (var db = Fixture.CreateContext())
        {
            var repo = new ReservationRepository(db);
            var persistida = await repo.GetByCodeAsync(reserva.Code, CancellationToken.None);
            persistida!.Cancel(trip, DateTimeOffset.UtcNow);
            await repo.SaveChangesAsync(CancellationToken.None);
        }

        await using (var db = Fixture.CreateContext())
        {
            var resultado = await new ReservationRepository(db).AddAsync(NovaReserva(trip, 15), CancellationToken.None);
            Assert.Equal(ReservationInsertResult.Inserted, resultado);
        }
    }

    [Fact]
    public async Task AddAsync_CodigoDuplicado_RetornaCodeCollision()
    {
        var trip = await SeedTripAsync();
        var original = NovaReserva(trip, 1);

        await using (var db = Fixture.CreateContext())
        {
            await new ReservationRepository(db).AddAsync(original, CancellationToken.None);
        }

        PassengerName.TryCreate("Ana Lima", out var name);
        Cpf.TryCreate("52998224725", out var cpf);
        var duplicada = Reservation.Rehydrate(
            Guid.NewGuid(), original.Code, trip.Id, 2, name!, cpf!,
            ReservationStatus.Confirmed, DateTimeOffset.UtcNow, null);

        await using (var db = Fixture.CreateContext())
        {
            var resultado = await new ReservationRepository(db).AddAsync(duplicada, CancellationToken.None);
            Assert.Equal(ReservationInsertResult.CodeCollision, resultado);
        }
    }
}
