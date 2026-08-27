using FilesProcessor.WebApi.Application.Processing;

namespace FilesProcessor.WebApi.Tests.Mocks;

/// <summary>
/// In-memory stand-in for IProcessingQueue. Records which file IDs were
/// enqueued so tests can assert on them — nothing is actually processed.
/// </summary>
public class FakeProcessingQueue : IProcessingQueue
{
    public List<Guid> EnqueuedFileIds { get; } = [];

    public void EnqueueFileProcessing(Guid fileId)
        => EnqueuedFileIds.Add(fileId);
}
