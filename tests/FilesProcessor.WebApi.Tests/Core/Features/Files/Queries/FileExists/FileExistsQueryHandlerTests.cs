using FilesProcessor.WebApi.Core.Dtos.Files;
using FilesProcessor.WebApi.Core.Features.Files.Queries.FileExists;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using FilesProcessor.WebApi.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FilesProcessor.WebApi.Tests.Core.Features.Files.Queries.FileExists;

public class FileExistsQueryHandlerTests : IDisposable
{
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;

    public FileExistsQueryHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<LocalFile> SeedFileAsync(Guid? id = null)
    {
        var dto = new CreateFileDto(
            "photo.png", "new.png", "storage-key", 11,
            FileType.Image, "image/png", ".png", "checksum", string.Empty);

        var entity = LocalFile.Create(dto);
        if (id is not null) entity.Id = id.Value;

        _db.Files.Add(entity);
        await _db.SaveChangesAsync(CancellationToken.None);
        return entity;
    }

    [Fact]
    public async Task Handle_returns_true_when_file_exists()
    {
        // Arrange
        var seeded = await SeedFileAsync();
        var sut = new FileExistsQueryHandler(_db);

        // Act
        var exists = await sut.Handle(new FileExistsQuery(seeded.Id), CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task Handle_returns_false_when_file_id_unknown()
    {
        // Arrange
        var sut = new FileExistsQueryHandler(_db);

        // Act
        var exists = await sut.Handle(new FileExistsQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task Handle_returns_false_when_file_is_soft_deleted()
    {
        // Arrange — the global query filter (!IsDeleted) must hide it
        var seeded = await SeedFileAsync();
        _db.Files.Remove(seeded);
        await _db.SaveChangesAsync(CancellationToken.None);   // AppDbContext turns this into a soft delete
        var sut = new FileExistsQueryHandler(_db);

        // Act
        var exists = await sut.Handle(new FileExistsQuery(seeded.Id), CancellationToken.None);

        // Assert
        Assert.False(exists);
    }
}
