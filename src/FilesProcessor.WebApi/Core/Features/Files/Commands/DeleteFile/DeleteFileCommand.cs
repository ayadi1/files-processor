using MediatR;

namespace FilesProcessor.WebApi.Core.Features.Files.Commands.DeleteFile;

public record DeleteFileCommand(Guid Id) : IRequest;
