using FilesProcessor.WebApi.Domain.Common;

namespace FilesProcessor.WebApi.Domain.Entities
{
    public class LocalFile : ISoftDelete
    {
        public Guid Id { get; set; }
        public string RealFileName { get; set; } = string.Empty;
        public string NewFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string EncryptionKey { get; set; } = string.Empty;
        public long Size { get; set; }
        public FileType Type { get; set; }
        public string MimeTime { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;
        public Guid UploadedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<Variant> Variants { get; set; } = [];
    }
}