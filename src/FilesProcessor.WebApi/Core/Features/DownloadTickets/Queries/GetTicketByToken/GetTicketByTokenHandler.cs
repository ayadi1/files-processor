using FilesProcessor.WebApi.Core.Features.Files.Queries.GetFileById;
using FilesProcessor.WebApi.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FilesProcessor.WebApi.Core.Features.DownloadTickets.Queries.GetTicketByToken;

public class GetTicketByTokenHandler(AppDbContext appDbContext, ISender sender, ILogger<GetTicketByTokenHandler> logger) : IRequestHandler<GetTicketByTokenQuery, GetTicketByTokenResult>
{
    public async Task<GetTicketByTokenResult> Handle(GetTicketByTokenQuery request, CancellationToken cancellationToken)
    {
        var ticket = await appDbContext.DownloadTickets.FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (ticket is null || ticket.ExpiresAt < DateTime.UtcNow)
        {
            logger.LogWarning("Download ticket {TokenMasked} not resolved (not found or expired)", request.Token);
            throw new FileNotFoundException("Download link is invalid or has expired.");
        }

        logger.LogInformation("Download ticket {TicketId} resolved for file {FileId}", ticket.Id, ticket.FileId);

        var file = await sender.Send(new GetFileByIdQuery(ticket.FileId, request.Resolution), cancellationToken);

        return new GetTicketByTokenResult(file.Content, file.FileName, file.ContentType, file.Size);
    }
}
