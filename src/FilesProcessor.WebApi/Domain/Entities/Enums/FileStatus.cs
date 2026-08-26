namespace FilesProcessor.WebApi.Domain.Entities.Enums;

public enum FileStatus
{
    Pending,      // uploaded, waiting for the worker
    Processing,   // worker is generating variants / encrypting
    Ready,        // done, downloadable
    Failed        // worker gave up
}
