namespace OniBusExpress.Domain.Passengers;

public sealed record PassengerName
{
    public string Value { get; }

    private PassengerName(string value) => Value = value;

    public static bool TryCreate(string? input, out PassengerName? name)
    {
        name = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        name = new PassengerName(input.Trim());
        return true;
    }

    public static PassengerName FromPersistence(string value) =>
        TryCreate(value, out var name)
            ? name!
            : throw new InvalidOperationException("Nome de passageiro persistido em formato inválido.");
}
