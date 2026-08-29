using FilesProcessor.WebApi.Domain.Entities.Enums;
using MediatR;

namespace FilesProcessor.WebApi.Core.Features.DownloadTickets.Queries.GetTicketByToken;

public record GetTicketByTokenQuery(Guid Token, Resolution? Resolution): IRequest<GetTicketByTokenResult>;
