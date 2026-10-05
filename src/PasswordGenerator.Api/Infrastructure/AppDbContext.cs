using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Domain;

namespace PasswordGenerator.Api.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<StoredPassword> Passwords => Set<StoredPassword>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var password = modelBuilder.Entity<StoredPassword>();
        password.ToTable("Passwords");
        password.HasKey(p => p.Id);
        password.Property(p => p.Id).ValueGeneratedNever();
        password.Property(p => p.Value).IsRequired().HasMaxLength(128);
        // SQLite não guarda o DateTimeKind; sem isso a data volta sem o "Z" de UTC
        password.Property(p => p.CreatedAt)
            .IsRequired()
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
}
