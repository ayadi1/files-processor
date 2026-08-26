using System.Text;

using FilesProcessor.WebApi.Core.Options.Upload;
using FilesProcessor.WebApi.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Xunit;

namespace FilesProcessor.WebApi.Tests.Infrastructure.Storage;

/// <summary>
/// TDD tests for <see cref="LocalDiskFileStorage"/>.
///
/// Design assumed by these tests (option B from our discussion):
///   - <c>SaveAsync</c> returns a Guid-based storage key (a single relative
///     path segment, e.g. "<c>{guid}{extension}</c>") that, combined with
///     <c>UploadOptions.RootPath</c>, locates the file on disk.
///   - The caller owns and disposes the stream returned by <c>ReadAsync</c>.
///   - <c>DeleteAsync</c> is idempotent: deleting a missing file does NOT throw.
///
/// Every test is currently RED: the methods throw NotImplementedException.
/// Implement them one by one to turn the corresponding test GREEN.
/// </summary>
public sealed class LocalDiskFileStorageTests : IDisposable
{
    // Unique temp root per test instance -> isolated, no litter left behind.
    private readonly string _rootPath =
        Path.Combine(Path.GetTempPath(), "fp-tests-" + System.Guid.NewGuid().ToString("N"));

    private readonly LocalDiskFileStorage _sut;

    public LocalDiskFileStorageTests()
    {
        var logger = NullLogger<LocalDiskFileStorage>.Instance;
        Directory.CreateDirectory(_rootPath);
        var options = Options.Create(
            new UploadOptions { RootPath = _rootPath, MaxFileBytes = 1024 * 1024 });
        _sut = new LocalDiskFileStorage(options, logger);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    // --- SaveAsync ---------------------------------------------------------

    [Fact]
    public async Task SaveAsync_returns_non_empty_key_and_writes_file_under_root()
    {
        var content = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));

        var key = await _sut.SaveAsync(content, "photo.png", CancellationToken.None);

        Assert.False(string.IsNullOrEmpty(key));
        var fullPath = Path.Combine(_rootPath, key);
        Assert.True(File.Exists(fullPath));
        Assert.Equal("hello world", await File.ReadAllTextAsync(fullPath));
    }

    // --- ExistsAsync -------------------------------------------------------

    [Fact]
    public async Task ExistsAsync_is_true_after_save_and_false_for_unknown_key()
    {
        var key = await _sut.SaveAsync(
            new MemoryStream(Encoding.UTF8.GetBytes("data")), "f.bin", CancellationToken.None);

        Assert.True(await _sut.ExistsAsync(key, CancellationToken.None));
        Assert.False(await _sut.ExistsAsync("does-not-exist-key", CancellationToken.None));
    }

    // --- ReadAsync ---------------------------------------------------------

    [Fact]
    public async Task ReadAsync_returns_stream_with_the_saved_bytes()
    {
        var bytes = Encoding.UTF8.GetBytes("the quick brown fox");
        var key = await _sut.SaveAsync(new MemoryStream(bytes), "f.txt", CancellationToken.None);

        await using var stream = await _sut.ReadAsync(key, CancellationToken.None);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        Assert.Equal(bytes, ms.ToArray());
    }

    // --- DeleteAsync -------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_removes_file_and_is_idempotent()
    {
        var key = await _sut.SaveAsync(
            new MemoryStream(Encoding.UTF8.GetBytes("x")), "f.dat", CancellationToken.None);
        var fullPath = Path.Combine(_rootPath, key);

        await _sut.DeleteAsync(key, CancellationToken.None);
        Assert.False(File.Exists(fullPath));

        // Second delete must NOT throw (interface contract: no-op if gone).
        await _sut.DeleteAsync(key, CancellationToken.None);
    }
}
