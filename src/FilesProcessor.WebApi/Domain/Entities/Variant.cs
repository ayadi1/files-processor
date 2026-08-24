namespace FilesProcessor.WebApi.Domain.Entities
{
    public class Variant
    {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public Resolution Resolution { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public required LocalFile File { get; set; }
    }
}