using System.Text;
using FilesProcessor.WebApi.Core.Dtos.DownloadTickets;
using FilesProcessor.WebApi.Core.Dtos.Files;
using FilesProcessor.WebApi.Core.Features.DownloadTickets.Queries.GetTicketByToken;
using FilesProcessor.WebApi.Core.Features.Files.Queries.GetFileById;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using FilesProcessor.WebApi.Infrastructure;
using FilesProcessor.WebApi.Tests.Mocks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FilesProcessor.WebApi.Tests.Core.Features.DownloadTickets.Queries.GetTicketByToken;

public class GetTicketByTokenHandlerTests : IDisposable
{
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;
    private readonly FakeSender _fakeSender = new();

    public GetTicketByTokenHandlerTests()
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

    private GetTicketByTokenHandler CreateSut() =>
        new(_db, _fakeSender, NullLogger<GetTicketByTokenHandler>.Instance);

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

    private async Task<DownloadTicket> SeedTicketAsync(Guid fileId, DateTime expiresAt)
    {
        var ticket = DownloadTicket.Create(
            new CreateDownloadTicketDto(Guid.NewGuid(), expiresAt, fileId));
        _db.DownloadTickets.Add(ticket);
        await _db.SaveChangesAsync(CancellationToken.None);
        return ticket;
    }

    private FakeSender WithFileResult(byte[] bytes = null!)
    {
        bytes ??= Encoding.UTF8.GetBytes("ticket-bytes");
        _fakeSender.NextFileResult =
            new GetFileByIdResult(new MemoryStream(bytes), "photo.png", "image/png", bytes.Length);
        return _fakeSender;
    }

    [Fact]
    public async Task Handle_resolves_valid_ticket_and_delegates_to_file_query()
    {
        // Arrange
        var seeded = await SeedFileAsync();
        await SeedTicketAsync(seeded.Id, DateTime.UtcNow.AddMinutes(10));
        var fake = WithFileResult();
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetTicketByTokenQuery(_db.DownloadTickets.Single().Token, null),
            CancellationToken.None);

        // Assert — result carries the file content through
        Assert.Equal("photo.png", result.FileName);
        Assert.Equal("image/png", result.ContentType);
        Assert.Equal("ticket-bytes", Encoding.UTF8.GetString(GetBytes(result.Content)));

        // Assert — the delegated query targeted the ticket's file
        var sent = Assert.IsType<GetFileByIdQuery>(fake.SentRequests.Single());
        Assert.Equal(_db.DownloadTickets.Single().FileId, sent.Id);
        Assert.Null(sent.Resolution);
    }

    [Fact]
    public async Task Handle_throws_filenotfound_when_token_unknown()
    {
        // Arrange
        var sut = CreateSut();

        // Act + Assert — unknown token maps to 404 like a missing file
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.Handle(new GetTicketByTokenQuery(Guid.NewGuid(), null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_filenotfound_when_ticket_is_expired()
    {
        // Arrange — ticket exists but its expiry has passed
        var seeded = await SeedFileAsync();
        await SeedTicketAsync(seeded.Id, DateTime.UtcNow.AddSeconds(-1));

        var sut = CreateSut();

        // Act + Assert — expired and unknown must be indistinguishable: 404 both
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.Handle(new GetTicketByTokenQuery(_db.DownloadTickets.Single().Token, null),
                CancellationToken.None));

        // No delegation happened — the file query was never sent
        Assert.Empty(_fakeSender.SentRequests);
    }

    [Fact]
    public async Task Handle_forwards_resolution_to_the_file_query()
    {
        // Arrange
        var seeded = await SeedFileAsync();
        await SeedTicketAsync(seeded.Id, DateTime.UtcNow.AddMinutes(10));
        var fake = WithFileResult();
        var sut = CreateSut();

        // Act
        await sut.Handle(
            new GetTicketByTokenQuery(_db.DownloadTickets.Single().Token, Resolution.Thumbnail),
            CancellationToken.None);

        // Assert
        var sent = Assert.IsType<GetFileByIdQuery>(fake.SentRequests.Single());
        Assert.Equal(Resolution.Thumbnail, sent.Resolution);
    }

    private static byte[] GetBytes(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.Position = 0;
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
