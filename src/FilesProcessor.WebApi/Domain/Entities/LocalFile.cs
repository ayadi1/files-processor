using FilesProcessor.WebApi.Core.Dtos.Files;
using FilesProcessor.WebApi.Domain.Common;
using FilesProcessor.WebApi.Domain.Entities.Enums;

namespace FilesProcessor.WebApi.Domain.Entities;

public class LocalFile : ISoftDelete
{
    private LocalFile() { }
    public Guid Id { get; set; }
    public string RealFileName { get; set; } = string.Empty;
    public string NewFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string EncryptionKey { get; set; } = string.Empty;
    public long Size { get; set; }
    public FileType Type { get; set; }
    public FileStatus Status { get; set; }
    public string MimeTime { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<Variant> Variants { get; set; } = [];

    public static LocalFile Create(CreateFileDto createFileDto)
    {
        return new()
        {
            Id = Guid.CreateVersion7(),
            CreatedAt = DateTime.UtcNow,
            UploadedBy = Guid.CreateVersion7(),
            Status = FileStatus.Pending,          // new
            Size = createFileDto.Size,            // new
            Type = createFileDto.Type,            // new
            Variants = [],
            Checksum = createFileDto.Checksum,
            EncryptionKey = createFileDto.EncryptionKey,
            Extension = createFileDto.Extension,
            FilePath = createFileDto.FilePath,
            MimeTime = createFileDto.MimeTime,
            NewFileName = createFileDto.NewFileName,
            RealFileName = createFileDto.RealFileName,
        };
    }
}