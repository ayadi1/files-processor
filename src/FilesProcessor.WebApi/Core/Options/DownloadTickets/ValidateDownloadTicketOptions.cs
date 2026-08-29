using Microsoft.Extensions.Options;

namespace FilesProcessor.WebApi.Core.Options.DownloadTickets;

public class ValidateDownloadTicketOptions : IValidateOptions<DownloadTicketOptions>
{
    public ValidateOptionsResult Validate(string? name, DownloadTicketOptions options)
    {
        var failures = new List<string>();

        // ValidForMinutes
        if (options.ValidForMinutes <= 0)
        {
            failures.Add("ValidForMinutes is required.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
