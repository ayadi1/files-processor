using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilesProcessor.WebApi.Core.Options.Upload;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace FilesProcessor.WebApi.Core.Configurations;

public class ConfigureUploadFormLimits(IOptions<UploadOptions> upload) : IConfigureOptions<FormOptions>
{
    private readonly UploadOptions _upload = upload.Value;

    public void Configure(FormOptions o) => o.MultipartBodyLengthLimit = _upload.MaxFileBytes;
}
