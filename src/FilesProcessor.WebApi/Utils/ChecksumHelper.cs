using System.Security.Cryptography;

namespace FilesProcessor.WebApi.Utils;

public class ChecksumHelper
{
    public static async Task<string> ComputeChecksumAsync(Stream stream, CancellationToken ct)
    {
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
