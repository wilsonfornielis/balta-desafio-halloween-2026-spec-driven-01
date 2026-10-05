namespace PasswordGenerator.Api.Domain;

public class StoredPassword
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
