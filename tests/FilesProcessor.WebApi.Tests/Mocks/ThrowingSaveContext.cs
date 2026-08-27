using FilesProcessor.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FilesProcessor.WebApi.Tests.Mocks;

/// <summary>
/// AppDbContext whose SaveChanges always throws, to simulate a DB failure
/// mid-operation (e.g. to test compensation logic in the upload handler).
/// </summary>
public sealed class ThrowingSaveContext(DbContextOptions<AppDbContext> o) : AppDbContext(o)
{
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new InvalidOperationException("simulated DB failure");
}
