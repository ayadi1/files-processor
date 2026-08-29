using FilesProcessor.WebApi.Core.Dtos.DownloadTickets;
using FilesProcessor.WebApi.Core.Features.DownloadTickets.Commands.CreateDownloadTicket;
using FilesProcessor.WebApi.Core.Features.DownloadTickets.Queries.GetTicketByToken;
using FilesProcessor.WebApi.Core.Options.DownloadTickets;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FilesProcessor.WebApi.Presentation.Controllers;

/// <summary>
/// Endpoints for creating expiring download tickets and downloading files through them.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DownloadTicketController(ISender sender, IOptions<DownloadTicketOptions> options) : ControllerBase
{
    private readonly DownloadTicketOptions _options = options.Value;

    /// <summary>
    /// Creates a short-lived download ticket for a file.
    /// </summary>
    /// <remarks>
    /// Returns a time-limited download URL instead of exposing the file id directly.
    /// The ticket expires after a configurable lifetime; the URL stops working then.
    /// </remarks>
    /// <param name="request">The ticket request containing the file id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The download URL and its expiration timestamp.</returns>
    /// <response code="201">The ticket was created; the download URL is returned.</response>
    /// <response code="404">No file with this id exists.</response>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateDownloadTicketCommand(request.FileId), ct);

        var url = Url.ActionLink(nameof(Download), values: new { token = result.Token });

        return Created(url!, new { downloadUrl = url, expiresAt = result.ExpiresAt });
    }

    /// <summary>
    /// Downloads a file using a download ticket token.
    /// </summary>
    /// <remarks>
    /// The token must belong to a ticket that has not expired. Optionally, an image
    /// <paramref name="resolution"/> can be requested; otherwise the original is served.
    /// </remarks>
    /// <param name="resolution">Optional resolution variant for images.</param>
    /// <param name="token">The ticket token from the download URL.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The file content with its original name and content type.</returns>
    /// <response code="200">The file content as a stream.</response>
    /// <response code="404">The ticket does not exist or has expired.</response>
    [HttpGet("{token:guid}")]
    public async Task<IActionResult> Download([FromQuery] Resolution? resolution, Guid token, CancellationToken ct)
    {
        var result = await sender.Send(new GetTicketByTokenQuery(token, resolution), ct);

        return new FileStreamResult(result.Content, result.ContentType)
        {
            FileDownloadName = result.FileName,
        };
    }
}
