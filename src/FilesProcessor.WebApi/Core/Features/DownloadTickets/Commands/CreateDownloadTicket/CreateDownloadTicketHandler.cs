using FilesProcessor.WebApi.Core.Dtos.DownloadTickets;
using FilesProcessor.WebApi.Core.Options.DownloadTickets;
using FilesProcessor.WebApi.Domain.Entities;
using FilesProcessor.WebApi.Infrastructure;
using MediatR;
using Microsoft.Extensions.Options;

namespace FilesProcessor.WebApi.Core.Features.DownloadTickets.Commands.CreateDownloadTicket;

public class CreateDownloadTicketHandler(ILogger<CreateDownloadTicketHandler> logger, AppDbContext appDbContext, IOptions<DownloadTicketOptions> options) : IRequestHandler<CreateDownloadTicketCommand, DownloadTicketResult>
{
    private readonly DownloadTicketOptions _options = options.Value;
    public async Task<DownloadTicketResult> Handle(CreateDownloadTicketCommand request, CancellationToken cancellationToken)
    {
        var file = await appDbContext.Files.FindAsync([request.FileId], cancellationToken);
        if (file is null)
        {
            logger.LogWarning("Download ticket rejected: file {FileId} not found", request.FileId);
            throw new FileNotFoundException($"File '{request.FileId}' was not found.");
        }

        var now = DateTime.UtcNow;
        var ticket = DownloadTicket.Create(new CreateDownloadTicketDto(Guid.NewGuid(), now.AddMinutes(_options.ValidForMinutes), file.Id));

        appDbContext.DownloadTickets.Add(ticket);
        await appDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Download ticket {TicketId} created for file {FileId}, expires at {ExpiresAt:O}",
            ticket.Id, file.Id, ticket.ExpiresAt);
        return new DownloadTicketResult(ticket.Token, ticket.ExpiresAt);

    }
}
