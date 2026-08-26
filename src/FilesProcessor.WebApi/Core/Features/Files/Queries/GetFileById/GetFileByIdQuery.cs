using System;
using MediatR;

namespace FilesProcessor.WebApi.Core.Features.Files.Queries.GetFileById;

public record GetFileByIdQuery(Guid Id) : IRequest<GetFileByIdResult>;