using FilesProcessor.WebApi.Core.Dtos.Files;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using FilesProcessor.WebApi.Infrastructure;
using FilesProcessor.WebApi.Storage;
using FilesProcessor.WebApi.Utils;
using MediatR;

namespace FilesProcessor.WebApi.Core.Features.Files.Commands.UploadFile;

public class UploadFileCommandHandler(AppDbContext appDbContext, IFileStorage fileStorage, ILogger<UploadFileCommandHandler> logger) : IRequestHandler<UploadFileCommand, UploadFileResult>
{
    public async Task<UploadFileResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        string? storageKey = null;
        try
        {
            // 1. stream request.File to storage -> get storage path
            var checksum = await ChecksumHelper.ComputeChecksumAsync(request.File.OpenReadStream(), cancellationToken);
            storageKey = await fileStorage.SaveAsync(request.File.OpenReadStream(), request.File.FileName, cancellationToken);

            // 2. build a CreateFileDto from the file metadata
            var fileDto = new CreateFileDto(
                request.File.FileName,
                $"{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}",
                storageKey,
                request.File.Length,
                FileTypeResolver.FromContentType(request.File.ContentType),
                request.File.ContentType,
                Path.GetExtension(request.File.FileName),
                checksum,
                string.Empty
                );

            // 3. LocalFile.Create(dto)  -> entity
            var entity = LocalFile.Create(fileDto);

            // 4. _db.Files.AddAsync(entity); await _db.SaveChangesAsync(ct);
            await appDbContext.Files.AddAsync(entity, cancellationToken);
            await appDbContext.SaveChangesAsync(cancellationToken);

            // 5. return new UploadFileResult(entity.Id, "Pending")
            return new UploadFileResult(entity.Id, entity.Status.ToString());
        }
        catch (Exception ex)
        {

            logger.LogError(ex, "Upload failed for {FileName}", request.File.FileName);

            // compensate: don't leave an orphaned file in storage
            if (storageKey is not null)
            {
                await fileStorage.DeleteAsync(storageKey, cancellationToken);
            }

            throw;
        }
    }
}
