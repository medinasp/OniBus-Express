using OniBusExpress.Domain.Trips;

namespace OniBusExpress.UnitTests.Trips;

public class TripTests
{
    private static readonly DateTimeOffset Agora = new(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);

    private static Trip TripPartindoEm(DateTimeOffset departure, int totalSeats = 40) =>
        new(Guid.NewGuid(), Guid.NewGuid(), departure, departure.AddHours(6), 150m, totalSeats);

    [Fact]
    public void HasDeparted_QuandoPartidaNoPassadoOuAgora_RetornaTrue()
    {
        Assert.True(TripPartindoEm(Agora.AddMinutes(-1)).HasDeparted(Agora));
        Assert.True(TripPartindoEm(Agora).HasDeparted(Agora));
    }

    [Fact]
    public void HasDeparted_QuandoPartidaNoFuturo_RetornaFalse()
    {
        Assert.False(TripPartindoEm(Agora.AddMinutes(1)).HasDeparted(Agora));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(40, true)]
    [InlineData(0, false)]
    [InlineData(41, false)]
    public void IsSeatWithinRange_ValidaLimites(int seat, bool esperado)
    {
        Assert.Equal(esperado, TripPartindoEm(Agora.AddDays(1)).IsSeatWithinRange(seat));
    }

    [Fact]
    public void IsCancellationAllowed_MaisDeDuasHorasAntes_RetornaTrue()
    {
        var trip = TripPartindoEm(Agora.AddHours(3));

        Assert.True(trip.IsCancellationAllowed(Agora));
    }

    [Fact]
    public void IsCancellationAllowed_MenosDeDuasHorasAntes_RetornaFalse()
    {
        var trip = TripPartindoEm(Agora.AddHours(1));

        Assert.False(trip.IsCancellationAllowed(Agora));
    }

    [Fact]
    public void IsCancellationAllowed_ExatamenteNoLimiteDeDuasHoras_RetornaTrue()
    {
        var trip = TripPartindoEm(Agora.AddHours(2));

        Assert.True(trip.IsCancellationAllowed(Agora));
    }
}
