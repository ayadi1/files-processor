using FilesProcessor.WebApi.Application.Processing;

namespace FilesProcessor.WebApi.Infrastructure.Processing;

public class HangfireProcessingQueue : IProcessingQueue
{
    public void EnqueueFileProcessing(Guid fileId)
    {
        throw new NotImplementedException();
    }
}
