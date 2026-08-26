using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesProcessor.WebApi.Core.Dtos;

public sealed record ApiErrorResponse(
int Status,
string Title,        // short reason phrase
string Detail,       // human-readable message
string? TraceId);    // helpful for debugging