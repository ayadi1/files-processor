using System.Text;
using FilesProcessor.WebApi.Core.Dtos.Files;
using FilesProcessor.WebApi.Core.Features.Files.Queries.GetFileById;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using FilesProcessor.WebApi.Infrastructure;
using FilesProcessor.WebApi.Tests.Mocks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FilesProcessor.WebApi.Tests.Core.Features.Files.Queries.GetFileById;

public class GetFileByIdQueryHandlerTests : IDisposable
{
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;

    public GetFileByIdQueryHandlerTests()
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
    public async Task Handle_returns_stream_and_metadata_for_existing_file()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("hello world");
        var fake = new FakeFileStorage();
        await fake.SaveAsync(new MemoryStream(bytes), "photo.png", CancellationToken.None);
        var storageKey = fake.LastSavedKey;
        var seeded = await SeedFileAsync(storageKey);

        var sut = new GetFileByIdQueryHandler(_db, fake, NullLogger<GetFileByIdQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetFileByIdQuery(seeded.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(seeded.RealFileName, result.FileName);
        Assert.Equal(seeded.MimeTime, result.ContentType);
        Assert.Equal(seeded.Size, result.Size);

        await using var stream = result.Content;
        using var reader = new StreamReader(stream);
        Assert.Equal("hello world", await reader.ReadToEndAsync());
    }

    [Fact]
    public async Task Handle_throws_filenotfound_when_id_unknown()
    {
        // Arrange
        var sut = new GetFileByIdQueryHandler(_db, new FakeFileStorage(),
            NullLogger<GetFileByIdQueryHandler>.Instance);

        // Act + Assert — GlobalExceptionHandler maps FileNotFoundException -> 404
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.Handle(new GetFileByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    private async Task<Variant> SeedVariantAsync(LocalFile file, Resolution resolution, string storageKey, long size = 7)
    {
        var variant = new Variant
        {
            Id = Guid.CreateVersion7(),
            File = file,
            FileId = file.Id,
            Resolution = resolution,
            Width = 150,
            Height = 150,
            Size = size,
            FilePath = storageKey,
            CreatedAt = DateTime.Now
        };
        file.Variants ??= [];
        file.Variants.Add(variant);
        await _db.SaveChangesAsync(CancellationToken.None);
        return variant;
    }

    [Fact]
    public async Task Handle_streams_requested_variant_with_variant_metadata_when_it_exists()
    {
        // Arrange
        var fake = new FakeFileStorage();
        await fake.SaveAsync(new MemoryStream(Encoding.UTF8.GetBytes("original-bytes")), "photo.png", CancellationToken.None);
        var seeded = await SeedFileAsync(fake.LastSavedKey);

        await fake.SaveAsync(new MemoryStream(Encoding.UTF8.GetBytes("thumbnail-bytes")), "thumbnail.jpg", CancellationToken.None);
        var thumbnailKey = fake.LastSavedKey;
        await SeedVariantAsync(seeded, Resolution.Thumbnail, thumbnailKey, size: 3);

        var sut = new GetFileByIdQueryHandler(_db, fake, NullLogger<GetFileByIdQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetFileByIdQuery(seeded.Id, Resolution.Thumbnail), CancellationToken.None);

        // Assert — streams the variant's content, not the original's
        await using var stream = result.Content;
        using var reader = new StreamReader(stream);
        Assert.Equal("thumbnail-bytes", await reader.ReadToEndAsync());
        Assert.Equal(3, result.Size);
        Assert.Equal(seeded.RealFileName, result.FileName);
        Assert.Equal(seeded.MimeTime, result.ContentType);
    }

    [Fact]
    public async Task Handle_throws_filenotfound_when_requested_variant_does_not_exist()
    {
        // Arrange — file exists but has no variants generated yet
        var fake = new FakeFileStorage();
        await fake.SaveAsync(new MemoryStream(Encoding.UTF8.GetBytes("original-bytes")), "photo.png", CancellationToken.None);
        var seeded = await SeedFileAsync(fake.LastSavedKey);

        var sut = new GetFileByIdQueryHandler(_db, fake, NullLogger<GetFileByIdQueryHandler>.Instance);

        // Act + Assert — missing variant maps to 404, same as a missing file
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.Handle(new GetFileByIdQuery(seeded.Id, Resolution.Medium), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_filenotfound_when_file_is_soft_deleted()
    {
        // Arrange
        var seeded = await SeedFileAsync("storage-key");
        _db.Files.Remove(seeded);
        await _db.SaveChangesAsync(CancellationToken.None);   // soft delete via AppDbContext override

        var sut = new GetFileByIdQueryHandler(_db, new FakeFileStorage(),
            NullLogger<GetFileByIdQueryHandler>.Instance);

        // Act + Assert — query filter hides soft-deleted rows
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.Handle(new GetFileByIdQuery(seeded.Id), CancellationToken.None));
    }
}
