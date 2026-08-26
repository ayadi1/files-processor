using FilesProcessor.WebApi.Application.Processing;

namespace FilesProcessor.WebApi.Infrastructure.Processing;

public class FileProcessor : IFileProcessor
{
    public Task ProcessAsync(Guid fileId)
    {
        throw new NotImplementedException();
    }
}
