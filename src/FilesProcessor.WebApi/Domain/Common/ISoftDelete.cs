namespace FilesProcessor.WebApi.Domain.Common
{
    public interface ISoftDelete
    {
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}