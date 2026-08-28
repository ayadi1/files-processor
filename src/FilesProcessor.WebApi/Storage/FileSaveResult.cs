namespace FilesProcessor.WebApi.Storage;

public record FileSaveResult(string StorageKey, string Checksum, long Size);
