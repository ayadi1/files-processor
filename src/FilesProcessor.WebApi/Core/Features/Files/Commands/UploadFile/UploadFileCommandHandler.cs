using FilesProcessor.WebApi.Application.Processing;
using FilesProcessor.WebApi.Core.Dtos.Files;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using FilesProcessor.WebApi.Infrastructure;
using FilesProcessor.WebApi.Storage;
using MediatR;

namespace FilesProcessor.WebApi.Core.Features.Files.Commands.UploadFile;

public class UploadFileCommandHandler(AppDbContext appDbContext, IFileStorage fileStorage, IProcessingQueue processingQueue, ILogger<UploadFileCommandHandler> logger) : IRequestHandler<UploadFileCommand, UploadFileResult>
{
    public async Task<UploadFileResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        string? storageKey = null;
        try
        {
            // 1. stream request.File to storage -> get storage path
            var saved = await fileStorage.SaveAsync(request.Content, request.FileName, cancellationToken);
            storageKey = saved.StorageKey;

            // 2. build a CreateFileDto from the file metadata
            var fileDto = new CreateFileDto(
                request.FileName,
                $"{Guid.NewGuid()}{Path.GetExtension(request.FileName)}",
                saved.StorageKey,
                saved.Size,
                FileTypeResolver.FromContentType(request.ContentType),
                request.ContentType,
                Path.GetExtension(request.FileName),
                saved.Checksum,
                string.Empty
                );

            // 3. LocalFile.Create(dto)  -> entity
            var entity = LocalFile.Create(fileDto);

            // 4. _db.Files.AddAsync(entity); await _db.SaveChangesAsync(ct);
            await appDbContext.Files.AddAsync(entity, cancellationToken);
            await appDbContext.SaveChangesAsync(cancellationToken);

            // fire file processing job
            processingQueue.EnqueueFileProcessing(entity.Id);

            // 5. return new UploadFileResult(entity.Id, "Pending")
            return new UploadFileResult(entity.Id, entity.Status.ToString());
        }
        catch (Exception ex)
        {

            logger.LogError(ex, "Upload failed for {FileName}", request.FileName);

            // compensate: don't leave an orphaned file in storage
            if (storageKey is not null)
            {
                await fileStorage.DeleteAsync(storageKey, cancellationToken);
            }

            throw;
        }
    }
}
