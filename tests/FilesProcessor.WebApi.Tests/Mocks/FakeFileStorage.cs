using System.Security.Cryptography;
using FilesProcessor.WebApi.Storage;

namespace FilesProcessor.WebApi.Tests.Mocks;

public class FakeFileStorage : IFileStorage
{
    public string LastSavedKey { get; private set; } = "";
    public List<string> DeletedKeys { get; } = [];
    private readonly Dictionary<string, byte[]> _store = new();

    public Task<FileSaveResult> SaveAsync(Stream content, string fileName, CancellationToken ct)
    {
        var key = Guid.NewGuid().ToString("N") + Path.GetExtension(fileName);
        using var ms = new MemoryStream();
        content.CopyTo(ms);                   // sync is fine in a fake
        var bytes = ms.ToArray();
        _store[key] = bytes;
        LastSavedKey = key;
        var checksum = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        return Task.FromResult(new FileSaveResult(key, checksum, bytes.Length));
    }
    public Task<Stream> ReadAsync(string storageKey, CancellationToken ct)
        => Task.FromResult<Stream>(new MemoryStream(_store[storageKey]));

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        _store.Remove(storageKey);
        DeletedKeys.Add(storageKey);          // for compensation assertion
        return Task.CompletedTask;
    }
    public Task<bool> ExistsAsync(string storageKey, CancellationToken ct)
        => Task.FromResult(_store.ContainsKey(storageKey));
}
