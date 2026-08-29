using FilesProcessor.WebApi.Core.Dtos.Files;
using FilesProcessor.WebApi.Core.Features.DownloadTickets.Commands.CreateDownloadTicket;
using FilesProcessor.WebApi.Core.Options.DownloadTickets;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using FilesProcessor.WebApi.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FilesProcessor.WebApi.Tests.Core.Features.DownloadTickets.Commands.CreateDownloadTicket;

public class CreateDownloadTicketHandlerTests : IDisposable
{
    private const int ValidForMinutes = 15;

    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;

    public CreateDownloadTicketHandlerTests()
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

    private CreateDownloadTicketHandler CreateSut() =>
        new(
            NullLogger<CreateDownloadTicketHandler>.Instance,
            _db,
            Options.Create(new DownloadTicketOptions { ValidForMinutes = ValidForMinutes }));

    private async Task<LocalFile> SeedFileAsync()
    {
        var dto = new CreateFileDto(
            "photo.png", "new.png", "storage-key", 11,
            FileType.Image, "image/png", ".png", "checksum", string.Empty);
        var entity = LocalFile.Create(dto);
        _db.Files.Add(entity);
        await _db.SaveChangesAsync(CancellationToken.None);
        return entity;
    }

    [Fact]
    public async Task Handle_creates_ticket_and_returns_token_expiring_validForMinutes_later()
    {
        // Arrange
        var seeded = await SeedFileAsync();
        var sut = CreateSut();

        // Act
        var before = DateTime.UtcNow;
        var result = await sut.Handle(new CreateDownloadTicketCommand(seeded.Id), CancellationToken.None);
        var after = DateTime.UtcNow;

        // Assert — returned contract
        Assert.NotEqual(Guid.Empty, result.Token);
        Assert.InRange(result.ExpiresAt, before.AddMinutes(ValidForMinutes), after.AddMinutes(ValidForMinutes));

        // Assert — persisted row matches what was returned
        var row = await _db.DownloadTickets.SingleAsync();
        Assert.Equal(result.Token, row.Token);
        Assert.Equal(result.ExpiresAt, row.ExpiresAt);
        Assert.Equal(seeded.Id, row.FileId);          // ticket points at the file
        Assert.InRange(row.CreatedAt, before, after);      // CreatedAt comes from the same clock reading
    }

    [Fact]
    public async Task Handle_throws_filenotfound_when_file_unknown_and_persists_nothing()
    {
        // Arrange
        var sut = CreateSut();

        // Act + Assert — GlobalExceptionHandler maps FileNotFoundException -> 404
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.Handle(new CreateDownloadTicketCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Empty(await _db.DownloadTickets.ToListAsync());
    }

    [Fact]
    public async Task Handle_throws_filenotfound_when_file_is_soft_deleted()
    {
        // Arrange — soft-deleted files must not get new tickets
        var seeded = await SeedFileAsync();
        _db.Files.Remove(seeded);
        await _db.SaveChangesAsync(CancellationToken.None);   // soft delete via AppDbContext override

        var sut = CreateSut();

        // Act + Assert — FindAsync respects the !IsDeleted query filter
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.Handle(new CreateDownloadTicketCommand(seeded.Id), CancellationToken.None));

        Assert.Empty(await _db.DownloadTickets.ToListAsync());
    }
}
