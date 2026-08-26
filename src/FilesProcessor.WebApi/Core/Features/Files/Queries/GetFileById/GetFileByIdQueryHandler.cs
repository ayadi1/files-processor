using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FilesProcessor.WebApi.Infrastructure;
using FilesProcessor.WebApi.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FilesProcessor.WebApi.Core.Features.Files.Queries.GetFileById;

public class GetFileByIdQueryHandler(AppDbContext appDbContext, IFileStorage fileStorage, ILogger<GetFileByIdQueryHandler> logger)
    : IRequestHandler<GetFileByIdQuery, GetFileByIdResult>
{
    public async Task<GetFileByIdResult> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        // FindAsync respects the global query filter (!IsDeleted), so soft-deleted
        // rows are treated as not found.
        var entity = await appDbContext.Files.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new FileNotFoundException($"File '{request.Id}' was not found.");
        }

        logger.LogInformation("Streaming file {FileId} from storage key {StorageKey}", request.Id, entity.FilePath);

        var stream = await fileStorage.ReadAsync(entity.FilePath, cancellationToken);
        return new GetFileByIdResult(stream, entity.RealFileName, entity.MimeTime, entity.Size);
    }
}