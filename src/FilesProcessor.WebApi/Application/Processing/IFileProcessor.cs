namespace FilesProcessor.WebApi.Application.Processing;

public interface IFileProcessor
{
    public Task ProcessAsync(Guid fileId);
}
