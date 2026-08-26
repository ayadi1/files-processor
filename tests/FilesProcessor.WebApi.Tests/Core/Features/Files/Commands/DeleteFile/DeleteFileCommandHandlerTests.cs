using System.Text;
using FilesProcessor.WebApi.Core.Dtos.Files;
using FilesProcessor.WebApi.Core.Features.Files.Commands.DeleteFile;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using FilesProcessor.WebApi.Infrastructure;
using FilesProcessor.WebApi.Tests.Mocks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FilesProcessor.WebApi.Tests.Core.Features.Files.Commands.DeleteFile;

public class DeleteFileCommandHandlerTests : IDisposable
{
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;

    public DeleteFileCommandHandlerTests()
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

    private async Task<LocalFile> SeedFileAsync(string storageKey)
    {
        var dto = new CreateFileDto(
            "photo.png", "new.png", storageKey, 11,
            FileType.Image, "image/png", ".png", "checksum", string.Empty);
        var entity = LocalFile.Create(dto);
        _db.Files.Add(entity);
        await _db.SaveChangesAsync(CancellationToken.None);
        return entity;
    }

    [Fact]
    public async Task Handle_soft_deletes_record_and_removes_from_storage()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("hello world");
        var fake = new FakeFileStorage();
        await fake.SaveAsync(new MemoryStream(bytes), "photo.png", CancellationToken.None);
        var storageKey = fake.LastSavedKey;
        var seeded = await SeedFileAsync(storageKey);

        var sut = new DeleteFileCommandHandler(_db, fake, NullLogger<DeleteFileCommandHandler>.Instance);

        // Act
        await sut.Handle(new DeleteFileCommand(seeded.Id), CancellationToken.None);

        // Assert — DB row is soft-deleted (query filter hides it now)
        var stillVisible = await _db.Files.AnyAsync(f => f.Id == seeded.Id, CancellationToken.None);
        Assert.False(stillVisible);

        // — storage file was removed
        Assert.Contains(storageKey, fake.DeletedKeys);
        Assert.False(await fake.ExistsAsync(storageKey, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_filenotfound_when_id_unknown()
    {
        // Arrange
        var sut = new DeleteFileCommandHandler(_db, new FakeFileStorage(),
            NullLogger<DeleteFileCommandHandler>.Instance);

        // Act + Assert — nothing to delete, and GlobalExceptionHandler maps this to 404
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.Handle(new DeleteFileCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_filenotfound_when_already_soft_deleted()
    {
        // Arrange
        var seeded = await SeedFileAsync("storage-key");
        _db.Files.Remove(seeded);
        await _db.SaveChangesAsync(CancellationToken.None);   // first soft delete

        var sut = new DeleteFileCommandHandler(_db, new FakeFileStorage(),
            NullLogger<DeleteFileCommandHandler>.Instance);

        // Act + Assert — the query filter hides the already-deleted row, so a second
        // delete looks like "not found".
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.Handle(new DeleteFileCommand(seeded.Id), CancellationToken.None));
    }
}
