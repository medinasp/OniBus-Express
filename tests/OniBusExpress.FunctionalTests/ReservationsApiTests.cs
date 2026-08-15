using System.Net;
using System.Net.Http.Json;
using OniBusExpress.Application.Reservations;
using OniBusExpress.Application.Trips;
using OniBusExpress.Infrastructure.Persistence;

namespace OniBusExpress.FunctionalTests;

public sealed class ReservationsApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ReservationsApiTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task PostReservation_ComDadosValidos_Retorna201ComLocation()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            tripId = SeedIds.TripFutura,
            seatNumber = 12,
            passenger = new { name = "Maria Silva", cpf = "111.444.777-35", email = "maria@exemplo.com" }
        };

        var response = await client.PostAsJsonAsync("/reservations", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/reservations/", response.Headers.Location!.ToString());

        var body = await response.Content.ReadFromJsonAsync<ReservationResponse>();
        Assert.Equal("Confirmed", body!.Status);
        Assert.Equal("***.***.**7-35", body.PassengerCpf);
        Assert.Equal("maria@exemplo.com", body.PassengerEmail);
    }

    [Fact]
    public async Task PostReservation_ComCpfInvalido_Retorna400()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            tripId = SeedIds.TripFutura,
            seatNumber = 13,
            passenger = new { name = "Maria Silva", cpf = "11111111111", email = "maria@exemplo.com" }
        };

        var response = await client.PostAsJsonAsync("/reservations", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteReservation_Existente_Retorna200Cancelled()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            tripId = SeedIds.TripFutura,
            seatNumber = 25,
            passenger = new { name = "Ana Lima", cpf = "529.982.247-25", email = "ana@exemplo.com" }
        };
        var created = await client.PostAsJsonAsync("/reservations", payload);
        var reserva = await created.Content.ReadFromJsonAsync<ReservationResponse>();

        var response = await client.DeleteAsync($"/reservations/{reserva!.Code}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ReservationResponse>();
        Assert.Equal("Cancelled", body!.Status);
    }

    [Fact]
    public async Task GetReservation_CodigoInexistente_Retorna404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/reservations/ZZZ-99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTrip_Inexistente_Retorna404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/trips/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTrips_SemResultados_Retorna200ComListaVazia()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/trips?origin=São Paulo&destination=Campinas&date=2020-01-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var trips = await response.Content.ReadFromJsonAsync<List<TripSummaryDto>>();
        Assert.Empty(trips!);
    }
}
