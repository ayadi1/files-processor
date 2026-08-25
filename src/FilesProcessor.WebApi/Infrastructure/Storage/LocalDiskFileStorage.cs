using FilesProcessor.WebApi.Core.Exceptions;
using FilesProcessor.WebApi.Core.Options.Upload;
using FilesProcessor.WebApi.Storage;
using FilesProcessor.WebApi.Utils;
using Microsoft.Extensions.Options;

namespace FilesProcessor.WebApi.Infrastructure.Storage
{
    public class LocalDiskFileStorage(IOptions<UploadOptions> options, ILogger<LocalDiskFileStorage> logger) : IFileStorage
    {
        private readonly UploadOptions _options = options.Value;

        public Task DeleteAsync(string storageKey, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var fullPath = Path.Combine(_options.RootPath, storageKey);

            try
            {
                File.Delete(fullPath);
            }
            catch (FileNotFoundException)
            {
                // already gone — contract says no-op. swallow.
            }
            catch (DirectoryNotFoundException)
            {
                // already gone — contract says no-op. swallow.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to delete {storageKey} from disk");
                throw;
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string storageKey, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var fullPath = Path.Combine(_options.RootPath, storageKey);
            return Task.FromResult(File.Exists(fullPath));
        }

        public Task<Stream> ReadAsync(string storageKey, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var fullPath = Path.Combine(_options.RootPath, storageKey);

            try
            {
                var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                return Task.FromResult<Stream>(fs);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error when reading: {storageKey}");
                throw;
            }
        }

        public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var fileFolderPath = $"{now:yyyy/MM/dd}";

            var ext = Path.GetExtension(fileName);
            var StorageKey = Path.Combine(fileFolderPath, $"{Guid.CreateVersion7():N}{ext}");

            var fullPath = Path.Combine(_options.RootPath, StorageKey);

            var dir = Path.GetDirectoryName(fullPath);
            FilesUtils.CreateFolderIfNotExists(dir!);

            try
            {
                await using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                var buffer = new byte[81920]; // 80 KiB — same default CopyToAsync uses
                long written = 0;
                int read;
                while ((read = await content.ReadAsync(buffer, ct)) > 0)
                {
                    written += read;
                    if (written > _options.MaxFileBytes)
                    {
                        throw new FileTooLargeException(_options.MaxFileBytes, written);
                    }

                    await fs.WriteAsync(buffer.AsMemory(0, read), ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to save {fileName} into disk");
                try { File.Delete(fullPath); } catch { }
                throw;
            }

            return StorageKey;
        }
    }
}