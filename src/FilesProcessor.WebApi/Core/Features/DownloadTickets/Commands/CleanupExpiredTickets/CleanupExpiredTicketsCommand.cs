using MediatR;

namespace FilesProcessor.WebApi.Core.Features.DownloadTickets.Commands.CleanupExpiredTickets;

public record CleanupExpiredTicketsCommand() : IRequest;
