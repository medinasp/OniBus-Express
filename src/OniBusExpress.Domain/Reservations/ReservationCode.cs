using System.Security.Cryptography;

namespace OniBusExpress.Domain.Reservations;

public sealed record ReservationCode
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const int LetterCount = 3;
    private const int DigitCount = 5;
    private const int Length = LetterCount + 1 + DigitCount;

    public string Value { get; }

    private ReservationCode(string value) => Value = value;

    public static ReservationCode Generate()
    {
        Span<char> buffer = stackalloc char[Length];

        for (var i = 0; i < LetterCount; i++)
        {
            buffer[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        buffer[LetterCount] = '-';

        for (var i = 0; i < DigitCount; i++)
        {
            buffer[LetterCount + 1 + i] = (char)('0' + RandomNumberGenerator.GetInt32(10));
        }

        return new ReservationCode(new string(buffer));
    }

    public static bool TryParse(string? input, out ReservationCode? code)
    {
        code = null;

        if (string.IsNullOrWhiteSpace(input) || input.Length != Length)
        {
            return false;
        }

        Span<char> buffer = stackalloc char[Length];

        for (var i = 0; i < LetterCount; i++)
        {
            var c = char.ToUpperInvariant(input[i]);
            if (!Alphabet.Contains(c))
            {
                return false;
            }

            buffer[i] = c;
        }

        if (input[LetterCount] != '-')
        {
            return false;
        }

        buffer[LetterCount] = '-';

        for (var i = LetterCount + 1; i < Length; i++)
        {
            var c = input[i];
            if (c is < '0' or > '9')
            {
                return false;
            }

            buffer[i] = c;
        }

        code = new ReservationCode(new string(buffer));
        return true;
    }
}
