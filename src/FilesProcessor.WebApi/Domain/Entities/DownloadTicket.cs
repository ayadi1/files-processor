using FilesProcessor.WebApi.Core.Dtos.DownloadTickets;

namespace FilesProcessor.WebApi.Domain.Entities;

public class DownloadTicket
{
    private DownloadTicket() { }
    public Guid Id { get; set; }
    public Guid Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid FileId { get; internal set; }

    public static DownloadTicket Create(CreateDownloadTicketDto createDownloadTicketDto)
    {
        return new()
        {
            Id = Guid.CreateVersion7(),
            CreatedAt = DateTime.Now,
            ExpiresAt = createDownloadTicketDto.ExpiresAt,
            FileId = createDownloadTicketDto.FileId,
            Token = createDownloadTicketDto.Token,
        };
    }
}

