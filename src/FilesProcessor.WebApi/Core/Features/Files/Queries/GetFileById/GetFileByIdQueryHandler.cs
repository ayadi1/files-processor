using FilesProcessor.WebApi.Domain.Entities.Enums;
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
        var entity = await appDbContext.Files.Include(e => e.Variants).FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new FileNotFoundException($"File '{request.Id}' was not found.");
        }

        logger.LogInformation("Streaming file {FileId} from storage key {StorageKey} and resolution {Resolution}", request.Id, entity.FilePath, request.Resolution);

        var filePath = entity.FilePath;
        var size = entity.Size;

        if (request.Resolution is not null && request.Resolution != Resolution.Original)
        {
            var variant = entity.Variants.FirstOrDefault(v => v.Resolution == request.Resolution);
            if (variant is null)
            {
                throw new FileNotFoundException($"Variant '{request.Resolution}' for file '{request.Id}' was not found.");
            }

            logger.LogInformation("Streaming variant {Resolution} for file {FileId} from storage key {StorageKey}",
                request.Resolution, request.Id, variant.FilePath);

            filePath = variant.FilePath;
            size = variant.Size;
        }
        else
        {
            logger.LogInformation("Streaming file {FileId} from storage key {StorageKey}", request.Id, entity.FilePath);
        }

        var stream = await fileStorage.ReadAsync(filePath, cancellationToken);

        return new GetFileByIdResult(stream, entity.RealFileName, entity.MimeTime, size);
    }
}
