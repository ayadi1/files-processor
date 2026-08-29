using MediatR;

namespace FilesProcessor.WebApi.Core.Features.DownloadTickets.Commands.CreateDownloadTicket;

public record CreateDownloadTicketCommand(Guid FileId): IRequest<DownloadTicketResult>;
