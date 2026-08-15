using Microsoft.AspNetCore.Http;
using OniBusExpress.Domain.Abstractions;

namespace OniBusExpress.Api.Http;

public static class ApiResults
{
    public const string TypeBase = "https://onibus.express/errors/";

    public static IResult Problem(DomainError error) =>
        Results.Problem(
            title: TitleFor(error.Code),
            detail: error.Description,
            statusCode: StatusFor(error.Type),
            type: TypeBase + error.Code);

    private static int StatusFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unprocessable => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string TitleFor(string code) => code switch
    {
        "validation-error" => "Requisição inválida",
        "resource-not-found" => "Recurso não encontrado",
        "seat-already-taken" => "Assento indisponível",
        "reservation-already-cancelled" => "Reserva já cancelada",
        "trip-in-the-past" => "Viagem no passado",
        "seat-out-of-range" => "Assento inexistente",
        "cancellation-window-closed" => "Fora da janela de cancelamento",
        _ => "Erro"
    };
}
