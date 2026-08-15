namespace OniBusExpress.Domain.Passengers;

public sealed record PassengerEmail
{
    public string Value { get; }

    private PassengerEmail(string value) => Value = value;

    public static bool TryCreate(string? input, out PassengerEmail? email)
    {
        email = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var trimmed = input.Trim();
        var at = trimmed.IndexOf('@');
        if (at <= 0 || at != trimmed.LastIndexOf('@') || at == trimmed.Length - 1)
        {
            return false;
        }

        var domain = trimmed[(at + 1)..];
        if (!domain.Contains('.') || domain.StartsWith('.') || domain.EndsWith('.') || trimmed.Contains(' '))
        {
            return false;
        }

        email = new PassengerEmail(trimmed);
        return true;
    }

    public static PassengerEmail FromPersistence(string value) =>
        TryCreate(value, out var email)
            ? email!
            : throw new InvalidOperationException("E-mail persistido em formato inválido.");
}
