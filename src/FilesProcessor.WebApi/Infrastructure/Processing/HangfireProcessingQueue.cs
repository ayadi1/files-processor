using FilesProcessor.WebApi.Application.Processing;
using Hangfire;

namespace FilesProcessor.WebApi.Infrastructure.Processing;

public class HangfireProcessingQueue : IProcessingQueue
{
    public void EnqueueFileProcessing(Guid fileId)
    {
        BackgroundJob.Enqueue<IFileProcessor>(processor => processor.ProcessAsync(fileId));
    }
}
