using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesProcessor.WebApi.Storage
{
    public interface IFileStorage
    {
        /// <summary>
        /// Writes the stream to storage. Returns the storage key/path the caller
        /// should persist (e.g. in LocalFile.Path) so it can find the file later.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fileName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct);

        /// <summary>
        /// Opens the stored file for reading. Caller owns + disposes the stream.
        /// </summary>
        /// <param name="storageKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Stream> ReadAsync(string storageKey, CancellationToken ct);

        /// <summary>
        /// Removes the stored file. No-op (not throw) if it's already gone.
        /// </summary>
        /// <param name="storageKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(string storageKey, CancellationToken ct);

        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="storageKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string storageKey, CancellationToken ct);
    }
}