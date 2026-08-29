using MediatR;

namespace FilesProcessor.WebApi.Core.Features.DownloadTickets.Commands.CleanupExpiredTickets;

public class CleanupExpiredTicketsHandler : IRequestHandler<CleanupExpiredTicketsCommand>
{
    public Task Handle(CleanupExpiredTicketsCommand request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
