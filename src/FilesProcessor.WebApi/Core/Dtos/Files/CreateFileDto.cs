using FilesProcessor.WebApi.Domain.Entities.Enums;

namespace FilesProcessor.WebApi.Core.Dtos.Files;

public record CreateFileDto(
string RealFileName,
string NewFileName,
string FilePath,
long Size,
FileType Type,
string MimeTime,
string Extension,
string Checksum,
string EncryptionKey
);
