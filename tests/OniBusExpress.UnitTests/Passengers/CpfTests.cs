using OniBusExpress.Domain.Passengers;

namespace OniBusExpress.UnitTests.Passengers;

public class CpfTests
{
    [Theory]
    [InlineData("11144477735")]
    [InlineData("52998224725")]
    public void TryCreate_ComCpfValido_RetornaTrueEExpoeValorNormalizado(string entrada)
    {
        var ok = Cpf.TryCreate(entrada, out var cpf);

        Assert.True(ok);
        Assert.NotNull(cpf);
        Assert.Equal(entrada, cpf!.Value);
    }

    [Fact]
    public void TryCreate_ComFormatacao_NormalizaParaSomenteDigitos()
    {
        var ok = Cpf.TryCreate("111.444.777-35", out var cpf);

        Assert.True(ok);
        Assert.Equal("11144477735", cpf!.Value);
    }

    [Theory]
    [InlineData("11144477736")]
    [InlineData("11144477715")]
    public void TryCreate_ComDigitoVerificadorErrado_RetornaFalse(string entrada)
    {
        var ok = Cpf.TryCreate(entrada, out var cpf);

        Assert.False(ok);
        Assert.Null(cpf);
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    public void TryCreate_ComDigitosRepetidos_RetornaFalse(string entrada)
    {
        var ok = Cpf.TryCreate(entrada, out var cpf);

        Assert.False(ok);
        Assert.Null(cpf);
    }

    [Theory]
    [InlineData("1114447773")]
    [InlineData("111444777355")]
    public void TryCreate_ComTamanhoDiferenteDeOnze_RetornaFalse(string entrada)
    {
        var ok = Cpf.TryCreate(entrada, out var cpf);

        Assert.False(ok);
        Assert.Null(cpf);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("111444777ab")]
    public void TryCreate_ComEntradaNulaVaziaOuNaoNumerica_RetornaFalse(string? entrada)
    {
        var ok = Cpf.TryCreate(entrada, out var cpf);

        Assert.False(ok);
        Assert.Null(cpf);
    }

    [Fact]
    public void Cpfs_ComMesmoValor_SaoIguais()
    {
        Cpf.TryCreate("111.444.777-35", out var a);
        Cpf.TryCreate("11144477735", out var b);

        Assert.Equal(a, b);
    }
}
