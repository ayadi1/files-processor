namespace FilesProcessor.WebApi.Domain.Entities.Enums;

public enum FileType
{
    Image,
    Video,
    Document,
    Audio,
    Archive,
    Other,
}

public static class FileTypeResolver
{
    public static FileType FromContentType(string contentType)
        => contentType?.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/png" or "image/webp" or "image/gif" => FileType.Image,
            "video/mp4" or "video/quicktime" => FileType.Video,
            "audio/mpeg" or "audio/wav" => FileType.Audio,
            "application/pdf" => FileType.Document,
            "application/zip" or "application/x-rar-compressed" => FileType.Archive,
            _ => FileType.Other,
        };
}
