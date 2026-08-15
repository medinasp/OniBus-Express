using OniBusExpress.Domain.Passengers;

namespace OniBusExpress.UnitTests.Passengers;

public class PassengerNameTests
{
    [Fact]
    public void TryCreate_ComNomeValido_RemoveEspacosNasBordas()
    {
        var ok = PassengerName.TryCreate("  Maria Silva  ", out var name);

        Assert.True(ok);
        Assert.Equal("Maria Silva", name!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_ComNomeVazioOuEspacos_RetornaFalse(string? entrada)
    {
        var ok = PassengerName.TryCreate(entrada, out var name);

        Assert.False(ok);
        Assert.Null(name);
    }
}
