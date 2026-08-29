namespace FilesProcessor.WebApi.Core.Features.DownloadTickets.Commands.CreateDownloadTicket;

public record DownloadTicketResult(Guid Token, DateTime ExpiresAt);
