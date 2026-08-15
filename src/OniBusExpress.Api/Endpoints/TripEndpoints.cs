using System.Globalization;
using OniBusExpress.Api.Http;
using OniBusExpress.Application.Abstractions;
using OniBusExpress.Application.Trips;
using OniBusExpress.Domain.Abstractions;

namespace OniBusExpress.Api.Endpoints;

public static class TripEndpoints
{
    public static IEndpointRouteBuilder MapTripEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/trips", async (string? origin, string? destination, string? date, ITripQueries queries, CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination) || string.IsNullOrWhiteSpace(date))
                {
                    return Results.Problem(
                        title: "Requisição inválida",
                        detail: "Os parâmetros origin, destination e date são obrigatórios.",
                        statusCode: StatusCodes.Status400BadRequest,
                        type: ApiResults.TypeBase + "validation-error");
                }

                if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    return Results.Problem(
                        title: "Requisição inválida",
                        detail: "O parâmetro date deve estar no formato YYYY-MM-DD.",
                        statusCode: StatusCodes.Status400BadRequest,
                        type: ApiResults.TypeBase + "validation-error");
                }

                var trips = await queries.SearchAsync(new TripSearch(origin, destination, parsedDate), cancellationToken);
                return Results.Ok(trips);
            })
            .WithName("SearchTrips")
            .WithSummary("Busca viagens por origem, destino e data.")
            .Produces<IReadOnlyList<TripSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags("Viagens");

        app.MapGet("/api/trips/{id:guid}", async (Guid id, ITripQueries queries, CancellationToken cancellationToken) =>
            {
                var details = await queries.GetDetailsAsync(id, cancellationToken);
                return details is null
                    ? ApiResults.Problem(DomainError.NotFound("Viagem não encontrada."))
                    : Results.Ok(details);
            })
            .WithName("GetTripDetails")
            .WithSummary("Detalha uma viagem, incluindo o mapa de assentos.")
            .Produces<TripDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Viagens");

        return app;
    }
}
