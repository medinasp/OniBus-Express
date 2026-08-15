namespace OniBusExpress.Domain.Passengers;

public sealed record Cpf
{
    private const int Length = 11;

    public string Value { get; }

    public string Masked => $"***.***.**{Value[8]}-{Value[9]}{Value[10]}";

    private Cpf(string value) => Value = value;

    public static bool TryCreate(string? input, out Cpf? cpf)
    {
        cpf = null;

        var digits = Normalize(input);
        if (digits is null || !HasValidCheckDigits(digits))
        {
            return false;
        }

        cpf = new Cpf(digits);
        return true;
    }

    public static Cpf FromPersistence(string value) =>
        TryCreate(value, out var cpf)
            ? cpf!
            : throw new InvalidOperationException("CPF persistido em formato inválido.");

    private static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        Span<char> buffer = stackalloc char[Length];
        var count = 0;

        foreach (var c in input)
        {
            if (c is '.' or '-' || char.IsWhiteSpace(c))
            {
                continue;
            }

            if (c is < '0' or > '9' || count == Length)
            {
                return null;
            }

            buffer[count++] = c;
        }

        if (count != Length || AllDigitsEqual(buffer))
        {
            return null;
        }

        return new string(buffer);
    }

    private static bool AllDigitsEqual(ReadOnlySpan<char> digits)
    {
        foreach (var c in digits)
        {
            if (c != digits[0])
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasValidCheckDigits(string digits)
    {
        var firstCheck = CheckDigit(digits, upTo: 9, startWeight: 10);
        var secondCheck = CheckDigit(digits, upTo: 10, startWeight: 11);

        return firstCheck == digits[9] - '0' && secondCheck == digits[10] - '0';
    }

    private static int CheckDigit(string digits, int upTo, int startWeight)
    {
        var sum = 0;
        for (var i = 0; i < upTo; i++)
        {
            sum += (digits[i] - '0') * (startWeight - i);
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
