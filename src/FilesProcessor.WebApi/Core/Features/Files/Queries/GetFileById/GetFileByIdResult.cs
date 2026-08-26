namespace FilesProcessor.WebApi.Core.Features.Files.Queries.GetFileById;

/// <summary>
/// Carries the open content stream plus the metadata the caller (controller)
/// needs to build a download response. The caller owns + disposes the stream.
/// </summary>
public record GetFileByIdResult(Stream Content, string FileName, string ContentType, long Size);
