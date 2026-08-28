using MediatR;

namespace FilesProcessor.WebApi.Core.Features.Files.Commands.UploadFile;

public record UploadFileCommand(Stream Content, string FileName, string ContentType) : IRequest<UploadFileResult>;
