namespace FilesProcessor.WebApi.Core.Dtos.DownloadTickets;

public record CreateDownloadTicketDto(
    Guid Token,
    DateTime ExpiresAt,
    Guid FileId
);
