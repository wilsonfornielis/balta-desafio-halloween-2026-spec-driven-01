using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Domain;
using PasswordGenerator.Api.Infrastructure;

namespace PasswordGenerator.Api.Application;

public class PasswordService(AppDbContext db, IPasswordGenerator generator)
{
    public const int DefaultLength = 16;

    public async Task<CreatePasswordResponse> CreateAsync(int? length, CancellationToken cancellationToken = default)
    {
        var size = length ?? DefaultLength;

        if (size is < PasswordGenerator.MinLength or > PasswordGenerator.MaxLength)
            throw new ValidationException(
                "length",
                $"O tamanho deve estar entre {PasswordGenerator.MinLength} e {PasswordGenerator.MaxLength}.");

        var password = new StoredPassword
        {
            Id = Guid.NewGuid(),
            Value = generator.Generate(size),
            CreatedAt = DateTime.UtcNow
        };

        db.Passwords.Add(password);
        await db.SaveChangesAsync(cancellationToken);

        return new CreatePasswordResponse(password.Id);
    }

    public async Task<PasswordResponse?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var guid))
            throw new ValidationException("id", "O id informado não é um GUID válido.");

        return await db.Passwords
            .AsNoTracking()
            .Where(p => p.Id == guid)
            .Select(p => new PasswordResponse(p.Id, p.Value, p.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
