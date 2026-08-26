using MediatR;

namespace FilesProcessor.WebApi.Core.Features.Files.Queries.FileExists;

public record FileExistsQuery(Guid Id) : IRequest<bool>;
