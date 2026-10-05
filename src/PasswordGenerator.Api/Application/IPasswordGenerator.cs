namespace PasswordGenerator.Api.Application;

public interface IPasswordGenerator
{
    string Generate(int length);
}
