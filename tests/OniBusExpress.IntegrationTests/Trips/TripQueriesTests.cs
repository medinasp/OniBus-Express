using OniBusExpress.Application.Trips;
using OniBusExpress.Domain.Passengers;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Domain.Trips;
using OniBusExpress.Infrastructure.Persistence;
using OniBusExpress.Infrastructure.Reservations;
using OniBusExpress.Infrastructure.Trips;
using OniBusExpress.IntegrationTests.Infrastructure;

namespace OniBusExpress.IntegrationTests.Trips;

[Collection(DatabaseCollection.Name)]
public sealed class TripQueriesTests : IntegrationTestBase
{
    public TripQueriesTests(PostgresFixture fixture) : base(fixture)
    {
    }

    private async Task SeedAsync()
    {
        await using var db = Fixture.CreateContext();
        await new DatabaseSeeder(db, TimeProvider.System).SeedAsync();
    }

    [Fact]
    public async Task ListAsync_AposSeed_RetornaRotas()
    {
        await SeedAsync();

        await using var db = Fixture.CreateContext();
        var rotas = await new RouteQueries(db).ListAsync(null, null, Pagination.From(null, null), CancellationToken.None);

        Assert.Equal(3, rotas.Count);
    }

    [Fact]
    public async Task SearchAsync_PorOrigem_RetornaViagensComAssentosDisponiveis()
    {
        await SeedAsync();

        await using var db = Fixture.CreateContext();
        var viagens = await new TripQueries(db)
            .SearchAsync(new TripSearch("são paulo", null, null), Pagination.From(null, null), CancellationToken.None);

        Assert.NotEmpty(viagens);
        Assert.All(viagens, v => Assert.Equal(v.TotalSeats, v.AvailableSeats));
    }

    [Fact]
    public async Task ListAsync_ComPageSize_LimitaAQuantidadeRetornada()
    {
        await SeedAsync();

        await using var db = Fixture.CreateContext();
        var primeiraPagina = await new RouteQueries(db)
            .ListAsync(null, null, Pagination.From(1, 2), CancellationToken.None);

        Assert.Equal(2, primeiraPagina.Count);
    }

    [Fact]
    public async Task GetDetailsAsync_ComReservaConfirmada_MarcaAssentoOcupado()
    {
        await SeedAsync();

        Trip trip;
        await using (var db = Fixture.CreateContext())
        {
            trip = await db.Trips.FindAsync(SeedIds.TripFutura) ?? throw new InvalidOperationException();
        }

        PassengerName.TryCreate("Carlos Dias", out var name);
        Cpf.TryCreate("11144477735", out var cpf);
        var reserva = Reservation.Create(trip, 5, name!, cpf!, DateTimeOffset.UtcNow).Value!;

        await using (var db = Fixture.CreateContext())
        {
            await new ReservationRepository(db).AddAsync(reserva, CancellationToken.None);
        }

        await using (var db = Fixture.CreateContext())
        {
            var detalhes = await new TripQueries(db).GetDetailsAsync(SeedIds.TripFutura, CancellationToken.None);

            Assert.NotNull(detalhes);
            Assert.Equal(trip.TotalSeats, detalhes!.Seats.Count);
            Assert.Equal(trip.TotalSeats - 1, detalhes.AvailableSeats);
            Assert.False(detalhes.Seats.Single(s => s.Number == 5).Available);
            Assert.True(detalhes.Seats.Single(s => s.Number == 6).Available);
        }
    }

    [Fact]
    public async Task GetDetailsAsync_ViagemInexistente_RetornaNull()
    {
        await using var db = Fixture.CreateContext();
        var detalhes = await new TripQueries(db).GetDetailsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(detalhes);
    }
}
