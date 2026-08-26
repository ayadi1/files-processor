using MediatR;

namespace FilesProcessor.WebApi.Core.Features.Files.Commands.UploadFile;

public record UploadFileCommand(IFormFile File) : IRequest<UploadFileResult>;
