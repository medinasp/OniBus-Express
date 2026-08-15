using FluentValidation;
using OniBusExpress.Api.Contracts;
using OniBusExpress.Api.Http;
using OniBusExpress.Application.Reservations;

namespace OniBusExpress.Api.Endpoints;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/reservations", async (
                CreateReservationRequest request,
                IValidator<CreateReservationRequest> validator,
                CreateReservation useCase,
                CancellationToken cancellationToken) =>
            {
                var validation = await validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return Results.ValidationProblem(
                        validation.ToDictionary(),
                        title: "Requisição inválida",
                        type: ApiResults.TypeBase + "validation-error");
                }

                var command = new CreateReservationCommand(request.TripId, request.SeatNumber, request.Passenger?.Name, request.Passenger?.Cpf);
                var result = await useCase.HandleAsync(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/reservations/{result.Value!.Code}", result.Value)
                    : ApiResults.Problem(result.Error!);
            })
            .WithName("CreateReservation")
            .WithSummary("Cria uma reserva para um assento de uma viagem.")
            .Produces<ReservationResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags("Reservas");

        app.MapGet("/api/reservations/{code}", async (string code, GetReservation useCase, CancellationToken cancellationToken) =>
            {
                var result = await useCase.HandleAsync(code, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result.Error!);
            })
            .WithName("GetReservation")
            .WithSummary("Recupera uma reserva pelo código (CPF mascarado).")
            .Produces<ReservationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Reservas");

        app.MapPost("/api/reservations/{code}/cancellation", async (string code, CancelReservation useCase, CancellationToken cancellationToken) =>
            {
                var result = await useCase.HandleAsync(code, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result.Error!);
            })
            .WithName("CancelReservation")
            .WithSummary("Cancela uma reserva pelo código.")
            .Produces<ReservationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags("Reservas");

        return app;
    }
}
