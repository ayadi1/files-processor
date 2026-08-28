using FilesProcessor.WebApi.Application.Processing;
using FilesProcessor.WebApi.Core.Options.Upload;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FilesProcessor.WebApi.Infrastructure.Processing;

public class FileProcessor(AppDbContext appDbContext, IOptions<UploadOptions> options, ILogger<FileProcessor> logger) : IFileProcessor
{
    private readonly UploadOptions _options = options.Value;
    public async Task ProcessAsync(Guid fileId)
    {
        var fileEntity = await appDbContext.Files.FirstOrDefaultAsync(e => e.Id == fileId);

        if (fileEntity is null)
        {
            logger.LogWarning($"there is no file with this id : {fileId} in the database");
            return;
        }

        logger.LogInformation("Processing file {FileId} (type: {Type})", fileId, fileEntity.Type);

        if (fileEntity.Type == FileType.Image)
        {
            await ProcessImage(fileEntity);
            return;
        }

    }

    private Dictionary<Resolution, (int Width, int Height)> resolutions = new()
    {
        [Resolution.Thumbnail] = (150, 150),
        [Resolution.Small] = (320, 240),
        [Resolution.Medium] = (640, 480),
        [Resolution.Large] = (1024, 768),
        [Resolution.ExtraLarge] = (1920, 1080)
    };

    private async Task ProcessImage(LocalFile fileEntity)
    {
        try
        {
            logger.LogInformation("Generating {VariantCount} variants for image {FileId} at {FilePath}", resolutions.Count, fileEntity.Id, fileEntity.FilePath);

            fileEntity.Status = FileStatus.Processing;
            await appDbContext.SaveChangesAsync();

            using Image baseImage = await Image.LoadAsync(Path.Combine(_options.RootPath, fileEntity.FilePath));

            foreach (var resolution in resolutions)
            {
                var variantKey = BuildVariantKey(fileEntity, resolution.Key);
                var fullPath = Path.Combine(_options.RootPath, variantKey);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                using var variant = baseImage.Clone(x => x.Resize(new ResizeOptions()
                {
                    Size = new Size()
                    {
                        Width = resolution.Value.Width,
                        Height = resolution.Value.Height
                    },
                    Mode = ResizeMode.Max
                }));
                await variant.SaveAsJpegAsync(fullPath);
                var variantEntity = new Variant
                {
                    Id = Guid.CreateVersion7(),
                    File = fileEntity,
                    FileId = fileEntity.Id,
                    Resolution = resolution.Key,
                    Width = variant.Width,
                    Height = variant.Height,
                    Size = new FileInfo(fullPath).Length,
                    FilePath = variantKey,
                    CreatedAt = DateTime.Now
                };
                fileEntity.Variants ??= new List<Variant>();
                fileEntity.Variants.Add(variantEntity);
                logger.LogDebug("Variant {Resolution} written to {VariantPath} ({Width}x{Height}", resolution.Key, fullPath, variant.Width, variant.Height);
            }

            fileEntity.Status = FileStatus.Ready;
            await appDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {

            logger.LogError(ex, "Processing failed for file {FileId}", fileEntity.Id);
            fileEntity.Status = FileStatus.Failed;
            await appDbContext.SaveChangesAsync();
            throw;
        }
    }

    private string BuildVariantKey(LocalFile fileEntity, Resolution resolution)
    {
        string fileName = $"{resolution}.jpg";
        return Path.Combine(fileEntity.Id.ToString(), fileName);
    }
}
