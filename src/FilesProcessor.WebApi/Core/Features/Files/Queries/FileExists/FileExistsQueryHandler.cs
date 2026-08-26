using FilesProcessor.WebApi.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FilesProcessor.WebApi.Core.Features.Files.Queries.FileExists;

/// <summary>
/// Checks whether a non-deleted file record exists for the given id.
/// The global query filter (!IsDeleted) in LocalFileConfiguration automatically
/// hides soft-deleted rows, so no extra predicate is needed.
/// </summary>
public class FileExistsQueryHandler(AppDbContext appDbContext) : IRequestHandler<FileExistsQuery, bool>
{
    public Task<bool> Handle(FileExistsQuery request, CancellationToken cancellationToken)
        => appDbContext.Files.AnyAsync(f => f.Id == request.Id, cancellationToken);
}
