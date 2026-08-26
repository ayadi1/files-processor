namespace FilesProcessor.WebApi.Application.Processing;

public interface IProcessingQueue
{
    void EnqueueFileProcessing(Guid fileId);
}
