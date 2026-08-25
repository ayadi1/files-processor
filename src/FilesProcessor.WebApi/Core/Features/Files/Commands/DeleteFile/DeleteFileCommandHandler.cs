using FilesProcessor.WebApi.Infrastructure;
using FilesProcessor.WebApi.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FilesProcessor.WebApi.Core.Features.Files.Commands.DeleteFile
{
    public class DeleteFileCommandHandler(AppDbContext appDbContext, IFileStorage fileStorage, ILogger<DeleteFileCommandHandler> logger)
        : IRequestHandler<DeleteFileCommand>
    {
        public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            // FindAsync would work too; FirstOrDefault is used for consistency with the read handler.
            var entity = await appDbContext.Files.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (entity is null)
            {
                throw new FileNotFoundException($"File '{request.Id}' was not found.");
            }

            var storageKey = entity.FilePath;

            // 1. Soft-delete the record. AppDbContext converts Remove into an update
            //    setting IsDeleted=true / DeletedAt=UtcNow.
            appDbContext.Files.Remove(entity);
            await appDbContext.SaveChangesAsync(cancellationToken);

            // 2. Best-effort removal of the stored bytes. The storage contract is
            //    idempotent (no-op if already gone), so a missing file is fine; other
            //    errors are logged but do not undo the soft-delete (the DB is the
            //    source of truth).
            try
            {
                await fileStorage.DeleteAsync(storageKey, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Soft-delete succeeded for {FileId} but storage cleanup of {StorageKey} failed", request.Id, storageKey);
            }
        }
    }
}