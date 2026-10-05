namespace PasswordGenerator.Api.Application;

public record CreatePasswordRequest(int? Length);

public record CreatePasswordResponse(Guid Id);

public record PasswordResponse(Guid Id, string Password, DateTime CreatedAt);
