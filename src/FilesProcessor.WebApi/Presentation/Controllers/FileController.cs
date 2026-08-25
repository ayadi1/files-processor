using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilesProcessor.WebApi.Core.Features.Files.Commands.UploadFile;
using FilesProcessor.WebApi.Core.Options.Upload;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FilesProcessor.WebApi.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController(ISender sender, IOptions<UploadOptions> options) : ControllerBase
    {
        private readonly UploadOptions _options = options.Value;

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken ct)
        {
            if (file.Length > _options.MaxFileBytes)
            {
                return BadRequest("File too large.");
            }

            var result = await sender.Send(new UploadFileCommand(file), ct);
            return Accepted(result);
        }
    }
}