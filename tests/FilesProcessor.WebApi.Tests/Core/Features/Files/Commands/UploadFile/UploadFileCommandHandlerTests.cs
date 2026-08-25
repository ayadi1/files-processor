using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FilesProcessor.WebApi.Core.Features.Files.Commands.UploadFile;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using FilesProcessor.WebApi.Infrastructure;
using FilesProcessor.WebApi.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FilesProcessor.WebApi.Tests.Core.Features.Files.Commands.UploadFile;

public class UploadFileCommandHandlerTests : IDisposable
{
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;

    public UploadFileCommandHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();                       // keep open for the test's lifetime
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();             // create schema from your model
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private IFormFile GetFormFile()
    {
        var bytes = Encoding.UTF8.GetBytes("hello world");
        return new FormFile(
            new MemoryStream(bytes), 0, bytes.Length, "file", "photo.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png",
        };
    }

    [Fact]
    public async Task Handle_happy_path_persists_file_and_returns_pending()
    {
        // Arrange
        var fake = new FakeFileStorage();
        var sut = new UploadFileCommandHandler(_db, fake, NullLogger<UploadFileCommandHandler>.Instance);

        var formFile = GetFormFile();

        // Act
        var result = await sut.Handle(new UploadFileCommand(formFile), CancellationToken.None);

        // result
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Pending", result.Status);

        // Assert
        var entity = await _db.Files.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(fake.LastSavedKey, entity!.FilePath);     // storage key stored
        Assert.Equal(FileType.Image, entity.Type);              // derived from image/png
        Assert.Equal("", entity.EncryptionKey);                 // empty for now
        Assert.Equal(64, entity.Checksum.Length);               // SHA-256 = 32 bytes = 64 hex chars
    }

    [Fact]
    public async Task Handle_db_failure_compensates_by_deleting_stored_file()
    {
        // Arrange
        var fake = new FakeFileStorage();
        var throwingDb = new ThrowingSaveContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        var sut = new UploadFileCommandHandler(throwingDb, fake,
            NullLogger<UploadFileCommandHandler>.Instance);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.Handle(new UploadFileCommand(GetFormFile()), CancellationToken.None));

        // Assert
        // storage ran before the DB threw...
        Assert.NotEmpty(fake.LastSavedKey);
        // ...and was cleaned up (compensation)
        Assert.Contains(fake.LastSavedKey, fake.DeletedKeys);
    }

    [Fact]
    public async Task Handle_stores_correct_sha256_checksum()
    {
        // Arrange
        var fake = new FakeFileStorage();
        var sut = new UploadFileCommandHandler(_db, fake,
            NullLogger<UploadFileCommandHandler>.Instance);

        var bytes = Encoding.UTF8.GetBytes("hello world");
        var formFile = new FormFile(
            new MemoryStream(bytes), 0, bytes.Length, "file", "photo.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png",
        };

        // Act
        // expected: SHA-256 of the exact bytes, lowercase hex
        var expected = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        var result = await sut.Handle(new UploadFileCommand(formFile), CancellationToken.None);

        // Assert
        var entity = await _db.Files.FindAsync(result.Id);
        Assert.Equal(expected, entity!.Checksum);
    }
}

sealed class ThrowingSaveContext(DbContextOptions<AppDbContext> o) : AppDbContext(o)
{
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new InvalidOperationException("simulated DB failure");
}
