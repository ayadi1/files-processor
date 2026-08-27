using FilesProcessor.WebApi.Application.Processing;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace FilesProcessor.WebApi.Infrastructure.Processing;

public class FileProcessor(AppDbContext appDbContext, ILogger<FileProcessor> logger) : IFileProcessor
{
    public async Task ProcessAsync(Guid fileId)
    {
        var fileEntity = await appDbContext.Files.FirstAsync(e => e.Id == fileId);

        if (fileEntity is null)
        {
            logger.LogWarning($"there is no file with this id : {fileId} in the database");
            return;
        }

        fileEntity.Variants = [
                new Variant() {
                    File = fileEntity,
                    FileId = fileId,
                    Id = Guid.CreateVersion7(),
                    CreatedAt = DateTime.Now,
                    FilePath = "TODO",
                    Height = 0,
                    Size = 0,
                    Width = 0,
                    Resolution = Resolution.Original
                }
        ];

        appDbContext.Update(fileEntity);

        await appDbContext.SaveChangesAsync();
    }
}
