using OniBusExpress.Domain.Abstractions;

namespace OniBusExpress.UnitTests.Abstractions;

public class DomainErrorTests
{
    public static IEnumerable<object[]> Catalogo()
    {
        yield return new object[] { DomainError.Validation("Nome obrigatório."), "validation-error", ErrorType.Validation };
        yield return new object[] { DomainError.NotFound("Reserva inexistente."), "resource-not-found", ErrorType.NotFound };
        yield return new object[] { DomainError.SeatAlreadyTaken(), "seat-already-taken", ErrorType.Conflict };
        yield return new object[] { DomainError.ReservationAlreadyCancelled(), "reservation-already-cancelled", ErrorType.Conflict };
        yield return new object[] { DomainError.TripInThePast(), "trip-in-the-past", ErrorType.Unprocessable };
        yield return new object[] { DomainError.SeatOutOfRange(99, 40), "seat-out-of-range", ErrorType.Unprocessable };
        yield return new object[] { DomainError.CancellationWindowClosed(), "cancellation-window-closed", ErrorType.Unprocessable };
    }

    [Theory]
    [MemberData(nameof(Catalogo))]
    public void Fabricas_ProduzemCodigoECategoriaDoCatalogo(DomainError error, string codigoEsperado, ErrorType categoriaEsperada)
    {
        Assert.Equal(codigoEsperado, error.Code);
        Assert.Equal(categoriaEsperada, error.Type);
        Assert.False(string.IsNullOrWhiteSpace(error.Description));
    }
}
