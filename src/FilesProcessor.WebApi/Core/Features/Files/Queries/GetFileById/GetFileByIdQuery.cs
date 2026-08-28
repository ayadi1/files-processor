using FilesProcessor.WebApi.Domain.Entities.Enums;
using MediatR;

namespace FilesProcessor.WebApi.Core.Features.Files.Queries.GetFileById;

public record GetFileByIdQuery(Guid Id, Resolution? Resolution = Resolution.Original) : IRequest<GetFileByIdResult>;
