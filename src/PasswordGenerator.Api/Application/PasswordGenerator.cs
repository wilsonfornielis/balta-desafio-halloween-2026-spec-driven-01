using System.Security.Cryptography;

namespace PasswordGenerator.Api.Application;

public class PasswordGenerator : IPasswordGenerator
{
    public const int MinLength = 16;
    public const int MaxLength = 128;

    public const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    public const string Digits = "0123456789";
    public const string Specials = "!@#$%^&*()-_=+[]{};:,.?/";

    private static readonly string[] Groups = [Uppercase, Lowercase, Digits, Specials];
    private static readonly string AllCharacters = string.Concat(Groups);

    public string Generate(int length)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, MinLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, MaxLength);

        var chars = new char[length];

        // Garante ao menos um caractere de cada grupo (RN02)
        for (var i = 0; i < Groups.Length; i++)
            chars[i] = Pick(Groups[i]);

        for (var i = Groups.Length; i < length; i++)
            chars[i] = Pick(AllCharacters);

        // Fisher–Yates para que os caracteres obrigatórios não fiquem sempre no início
        for (var i = length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static char Pick(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
}
