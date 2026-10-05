using Generator = PasswordGenerator.Api.Application.PasswordGenerator;

namespace PasswordGenerator.Tests.Unit;

public class PasswordGeneratorTests
{
    private const int Iterations = 200;
    private readonly Generator _generator = new();

    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(128)]
    public void Generate_ReturnsPasswordWithRequestedLength(int length)
    {
        var password = _generator.Generate(length);

        Assert.Equal(length, password.Length);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(129)]
    public void Generate_OutOfRangeLength_Throws(int length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Generate(length));
    }

    [Fact]
    public void Generate_ContainsAtLeastOneOfEachGroup()
    {
        for (var i = 0; i < Iterations; i++)
        {
            var password = _generator.Generate(Generator.MinLength);

            Assert.Contains(password, char.IsUpper);
            Assert.Contains(password, char.IsLower);
            Assert.Contains(password, char.IsDigit);
            Assert.Contains(password, c => Generator.Specials.Contains(c));
        }
    }

    [Fact]
    public void Generate_NeverContainsWhitespace()
    {
        for (var i = 0; i < Iterations; i++)
        {
            var password = _generator.Generate(Generator.MaxLength);

            Assert.DoesNotContain(password, char.IsWhiteSpace);
        }
    }

    [Fact]
    public void Generate_ReturnsDifferentPasswords()
    {
        var passwords = Enumerable.Range(0, Iterations)
            .Select(_ => _generator.Generate(Generator.MinLength))
            .ToHashSet();

        Assert.Equal(Iterations, passwords.Count);
    }
}
