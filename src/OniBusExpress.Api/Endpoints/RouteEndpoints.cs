using OniBusExpress.Application.Abstractions;
using OniBusExpress.Application.Trips;

namespace OniBusExpress.Api.Endpoints;

public static class RouteEndpoints
{
    public static IEndpointRouteBuilder MapRouteEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/routes", async (string? origin, string? destination, IRouteQueries queries, CancellationToken cancellationToken) =>
                Results.Ok(await queries.ListAsync(origin, destination, cancellationToken)))
            .WithName("ListRoutes")
            .WithSummary("Lista as rotas disponíveis.")
            .WithDescription("Retorna as rotas cadastradas, com filtro opcional por origem e destino.")
            .Produces<IReadOnlyList<RouteDto>>(StatusCodes.Status200OK)
            .WithTags("Rotas");

        return app;
    }
}
