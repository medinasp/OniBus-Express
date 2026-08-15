using System.Text.RegularExpressions;
using OniBusExpress.Domain.Reservations;

namespace OniBusExpress.UnitTests.Reservations;

public class ReservationCodeTests
{
    private static readonly Regex Formato = new("^[A-HJ-NP-Z]{3}-[0-9]{5}$");

    [Fact]
    public void Generate_ProduzSempreOFormatoValidoSemLetrasAmbiguas()
    {
        for (var i = 0; i < 1000; i++)
        {
            var code = ReservationCode.Generate();

            Assert.Matches(Formato, code.Value);
            Assert.DoesNotContain('I', code.Value);
            Assert.DoesNotContain('O', code.Value);
        }
    }

    [Fact]
    public void TryParse_ComCodigoValido_RetornaTrue()
    {
        var ok = ReservationCode.TryParse("ABC-12345", out var code);

        Assert.True(ok);
        Assert.Equal("ABC-12345", code!.Value);
    }

    [Fact]
    public void TryParse_ComMinusculas_NormalizaParaMaiusculas()
    {
        var ok = ReservationCode.TryParse("abc-12345", out var code);

        Assert.True(ok);
        Assert.Equal("ABC-12345", code!.Value);
    }

    [Theory]
    [InlineData("ABCD-12345")]
    [InlineData("AB-12345")]
    [InlineData("ABC-1234")]
    [InlineData("ABC-123456")]
    [InlineData("ABC12345")]
    [InlineData("123-45678")]
    [InlineData("ABC-12A45")]
    [InlineData(null)]
    [InlineData("")]
    public void TryParse_ComFormatoInvalido_RetornaFalse(string? entrada)
    {
        var ok = ReservationCode.TryParse(entrada, out var code);

        Assert.False(ok);
        Assert.Null(code);
    }

    [Theory]
    [InlineData("AIC-12345")]
    [InlineData("AOC-12345")]
    public void TryParse_ComLetraAmbigua_RetornaFalse(string entrada)
    {
        var ok = ReservationCode.TryParse(entrada, out var code);

        Assert.False(ok);
        Assert.Null(code);
    }

    [Fact]
    public void ReservationCodes_ComMesmoValor_SaoIguais()
    {
        ReservationCode.TryParse("ABC-12345", out var a);
        ReservationCode.TryParse("abc-12345", out var b);

        Assert.Equal(a, b);
    }
}
