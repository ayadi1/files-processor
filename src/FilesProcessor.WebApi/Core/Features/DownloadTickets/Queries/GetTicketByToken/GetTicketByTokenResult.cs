namespace FilesProcessor.WebApi.Core.Features.DownloadTickets.Queries.GetTicketByToken;

public record GetTicketByTokenResult(Stream Content, string FileName, string ContentType, long Size);
