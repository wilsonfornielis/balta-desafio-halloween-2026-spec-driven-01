namespace PasswordGenerator.Api.Application;

public class ValidationException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
