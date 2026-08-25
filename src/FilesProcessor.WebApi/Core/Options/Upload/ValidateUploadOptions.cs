using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace FilesProcessor.WebApi.Core.Options.Upload
{
    public class ValidateUploadOptions : IValidateOptions<UploadOptions>
    {
        public ValidateOptionsResult Validate(string? name, UploadOptions options)
        {
            var failures = new List<string>();

            // RootPath
            if (string.IsNullOrWhiteSpace(options.RootPath))
            {
                failures.Add("RootPath is required.");
            }
            else if (!Path.IsPathRooted(options.RootPath))
            {
                failures.Add($"RootPath '{options.RootPath}' must be an absolute path.");
            }
            else if (options.RootPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                failures.Add("RootPath contains invalid characters.");
            }

            // MaxFileBytes
            if (options.MaxFileBytes < 1 || options.MaxFileBytes > 1_073_741_824)
            {
                failures.Add("the file must be [1 byte → 1 GiB]");
            }

            return failures.Count > 0
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }
}