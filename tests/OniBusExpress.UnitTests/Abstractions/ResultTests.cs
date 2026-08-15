using OniBusExpress.Domain.Abstractions;

namespace OniBusExpress.UnitTests.Abstractions;

public class ResultTests
{
    [Fact]
    public void Success_ComValor_IndicaSucessoEExpoeValor()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_ComErro_IndicaFalhaEExpoeErro()
    {
        var error = DomainError.TripInThePast();

        var result = Result<int>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ResultSemValor_DistingueSucessoDeFalha()
    {
        var ok = Result.Success();
        var fail = Result.Failure(DomainError.ReservationAlreadyCancelled());

        Assert.True(ok.IsSuccess);
        Assert.False(fail.IsSuccess);
    }

    [Fact]
    public void ConversaoImplicita_DeValor_ProduzSucesso()
    {
        Result<string> result = "ok";

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public void ConversaoImplicita_DeErro_ProduzFalha()
    {
        Result<string> result = DomainError.SeatOutOfRange(99, 40);

        Assert.False(result.IsSuccess);
        Assert.Equal("seat-out-of-range", result.Error!.Code);
    }
}
