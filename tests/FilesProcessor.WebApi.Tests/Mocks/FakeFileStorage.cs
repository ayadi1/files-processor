using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilesProcessor.WebApi.Storage;

namespace FilesProcessor.WebApi.Tests.Mocks;

public class FakeFileStorage : IFileStorage
{
    public string LastSavedKey { get; private set; } = "";
    public List<string> DeletedKeys { get; } = [];
    private readonly Dictionary<string, byte[]> _store = new();

    public Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct)
    {
        var key = Guid.NewGuid().ToString("N") + Path.GetExtension(fileName);
        using var ms = new MemoryStream();
        content.CopyTo(ms);                   // sync is fine in a fake
        _store[key] = ms.ToArray();
        LastSavedKey = key;
        return Task.FromResult(key);
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