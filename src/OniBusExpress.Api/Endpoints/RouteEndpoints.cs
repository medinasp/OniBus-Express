using OniBusExpress.Application.Abstractions;
using OniBusExpress.Application.Trips;

namespace OniBusExpress.Api.Endpoints;

public static class RouteEndpoints
{
    public static IEndpointRouteBuilder MapRouteEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/routes", async (string? origin, string? destination, int? page, int? pageSize, IRouteQueries queries, CancellationToken cancellationToken) =>
                Results.Ok(await queries.ListAsync(origin, destination, Pagination.From(page, pageSize), cancellationToken)))
            .WithName("ListRoutes")
            .WithSummary("Lista as rotas disponíveis.")
            .WithDescription("Retorna as rotas cadastradas, com filtro opcional por origem e destino e paginação (page, pageSize).")
            .Produces<IReadOnlyList<RouteDto>>(StatusCodes.Status200OK)
            .WithTags("Rotas");

        return app;
    }
}
