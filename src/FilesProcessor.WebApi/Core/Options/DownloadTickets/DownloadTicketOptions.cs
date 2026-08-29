namespace FilesProcessor.WebApi.Core.Options.DownloadTickets;

public class DownloadTicketOptions
{
    public static string SectionName = "DownloadTickets";
    public int ValidForMinutes { get; init; }
}
