using FilesProcessor.WebApi.Core.Features.Files.Commands.DeleteFile;
using FilesProcessor.WebApi.Core.Features.Files.Commands.UploadFile;
using FilesProcessor.WebApi.Core.Features.Files.Queries.FileExists;
using FilesProcessor.WebApi.Core.Features.Files.Queries.GetFileById;
using FilesProcessor.WebApi.Core.Options.Upload;
using FilesProcessor.WebApi.Domain.Entities.Enums;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace FilesProcessor.WebApi.Presentation.Controllers;

/// <summary>
/// Endpoints for uploading, checking, downloading, and deleting files.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileController(ISender sender, IOptions<UploadOptions> options, ILogger<FileController> logger) : ControllerBase
{
    private readonly UploadOptions _options = options.Value;

    /// <summary>
    /// Uploads a file for asynchronous processing.
    /// </summary>
    /// <remarks>
    /// The API acknowledges the upload immediately (202 Accepted);
    /// processing (e.g. generating variants) happens in the background.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="202">Upload accepted and queued for processing.</response>
    /// <response code="400">The file is empty or exceeds the maximum allowed size.</response>
    [HttpPost]
    public async Task<IActionResult> Upload()
    {
        var ct = HttpContext.RequestAborted;

        if (!Request.HasFormContentType)
        {
            return BadRequest("Expected multipart/form-data.");
        }

        var boundary = Request.GetMultipartBoundary();
        var reader = new MultipartReader(boundary, Request.Body);

        Stream? content = null;
        string fileName = "", contentType = "";

        MultipartSection? section;
        while ((section = await reader.ReadNextSectionAsync(ct)) is not null)
        {
            var disposition = section.GetContentDispositionHeader();
            if (disposition is null || !disposition.FileName.HasValue)
                continue; // not a file part just skip text fields

            fileName = disposition.FileName.Value;
            contentType = section.ContentType ?? "application/octet-stream";
            content = section.Body;
            break;
        }

        if (content is null)
        {
            return BadRequest("No file part found.");
        }

        var result = await sender.Send(new UploadFileCommand(content, fileName, contentType), ct);
        return Accepted(result);
    }

    /// <summary>
    /// Checks whether a file exists.
    /// </summary>
    /// <remarks>
    /// Returns only status information — no body is sent. Useful for cheap
    /// existence checks without downloading the file.
    /// </remarks>
    /// <param name="id">The file identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The file exists.</response>
    /// <response code="404">No file with this id exists.</response>
    [HttpHead("{id:guid}")]
    public async Task<IActionResult> Exists(Guid id, CancellationToken ct)
    {
        var exists = await sender.Send(new FileExistsQuery(id), ct);
        return exists ? Ok() : NotFound();
    }

    /// <summary>
    /// Downloads a file by its identifier and resolution.
    /// </summary>
    /// <param name="resolution"></param>
    /// <param name="id">The file identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The file content with its original name and content type.</returns>
    /// <response code="200">The file content as a stream.</response>
    /// <response code="404">No file with this id exists.</response>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download([FromQuery] Resolution? resolution, Guid id, CancellationToken ct)
    {
        logger.LogInformation("resolution {s}", resolution);
        var result = await sender.Send(new GetFileByIdQuery(id, resolution), ct);
        return new FileStreamResult(result.Content, result.ContentType)
        {
            FileDownloadName = result.FileName,
        };
    }

    /// <summary>
    /// Deletes a file by its identifier (soft delete).
    /// </summary>
    /// <param name="id">The file identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The file was deleted.</response>
    /// <response code="404">No file with this id exists.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteFileCommand(id), ct);
        return NoContent();
    }
}
