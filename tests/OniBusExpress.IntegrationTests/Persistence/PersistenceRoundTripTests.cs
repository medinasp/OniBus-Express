using Microsoft.EntityFrameworkCore;
using OniBusExpress.Domain.Passengers;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Domain.Trips;
using OniBusExpress.IntegrationTests.Infrastructure;

namespace OniBusExpress.IntegrationTests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class PersistenceRoundTripTests : IntegrationTestBase
{
    public PersistenceRoundTripTests(PostgresFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Reserva_PersistidaELida_PreservaValueObjects()
    {
        var partida = DateTimeOffset.UtcNow.AddDays(1);
        var route = new Route(Guid.NewGuid(), "São Paulo", "Campinas", TimeSpan.FromMinutes(90));
        var trip = new Trip(Guid.NewGuid(), route.Id, partida, partida.AddHours(2), 45.90m, 40);
        PassengerName.TryCreate("Maria Silva", out var name);
        Cpf.TryCreate("11144477735", out var cpf);
        PassengerEmail.TryCreate("maria@exemplo.com", out var email);
        var reserva = Reservation.Create(trip, 12, name!, cpf!, email!, null, DateTimeOffset.UtcNow).Value!;

        await using (var db = Fixture.CreateContext())
        {
            db.Add(route);
            db.Add(trip);
            db.Add(reserva);
            await db.SaveChangesAsync();
        }

        await using (var db = Fixture.CreateContext())
        {
            var lida = await db.Reservations.SingleAsync(r => r.Id == reserva.Id);

            Assert.Equal("11144477735", lida.PassengerCpf.Value);
            Assert.Equal("Maria Silva", lida.PassengerName.Value);
            Assert.Equal("maria@exemplo.com", lida.PassengerEmail.Value);
            Assert.Equal(reserva.Code.Value, lida.Code.Value);
            Assert.Equal(ReservationStatus.Confirmed, lida.Status);
            Assert.Equal(12, lida.SeatNumber);
            Assert.Equal(trip.Id, lida.TripId);
        }
    }
}
