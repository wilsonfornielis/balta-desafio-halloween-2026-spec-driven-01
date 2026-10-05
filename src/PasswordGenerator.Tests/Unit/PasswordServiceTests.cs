using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Application;
using PasswordGenerator.Api.Infrastructure;
using Generator = PasswordGenerator.Api.Application.PasswordGenerator;

namespace PasswordGenerator.Tests.Unit;

public sealed class PasswordServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly PasswordService _service;

    public PasswordServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();

        _service = new PasswordService(_db, new Generator());
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithoutLength_UsesDefaultLength()
    {
        var created = await _service.CreateAsync(null);

        var stored = await _db.Passwords.FindAsync(created.Id);
        Assert.NotNull(stored);
        Assert.Equal(PasswordService.DefaultLength, stored.Value.Length);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(128)]
    public async Task CreateAsync_WithValidLength_PersistsPasswordWithThatLength(int length)
    {
        var created = await _service.CreateAsync(length);

        var stored = await _db.Passwords.FindAsync(created.Id);
        Assert.NotNull(stored);
        Assert.Equal(length, stored.Value.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(129)]
    public async Task CreateAsync_WithOutOfRangeLength_ThrowsValidationException(int length)
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(length));

        Assert.Equal("length", exception.Field);
        Assert.Empty(_db.Passwords);
    }

    [Fact]
    public async Task CreateAsync_TwiceInARow_ReturnsDifferentIdsAndPasswords()
    {
        var first = await _service.CreateAsync(null);
        var second = await _service.CreateAsync(null);

        Assert.NotEqual(first.Id, second.Id);
        Assert.NotEqual((await _db.Passwords.FindAsync(first.Id))!.Value, (await _db.Passwords.FindAsync(second.Id))!.Value);
    }

    [Fact]
    public async Task GetAsync_ExistingId_ReturnsStoredPassword()
    {
        var created = await _service.CreateAsync(null);
        var stored = await _db.Passwords.FindAsync(created.Id);

        var result = await _service.GetAsync(created.Id.ToString());

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(stored!.Value, result.Password);
        Assert.Equal(stored.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task GetAsync_ReturnsCreatedAtInUtc()
    {
        var created = await _service.CreateAsync(null);
        _db.ChangeTracker.Clear();

        var result = await _service.GetAsync(created.Id.ToString());

        Assert.Equal(DateTimeKind.Utc, result!.CreatedAt.Kind);
    }

    [Fact]
    public async Task GetAsync_UnknownId_ReturnsNull()
    {
        var result = await _service.GetAsync(Guid.NewGuid().ToString());

        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("123")]
    public async Task GetAsync_InvalidGuid_ThrowsValidationException(string id)
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.GetAsync(id));

        Assert.Equal("id", exception.Field);
    }
}
