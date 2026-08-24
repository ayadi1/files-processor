#!/usr/bin/env bash
# Run the FilesProcessor WebApi with the HTTPS profile.
set -euo pipefail

PROJECT="src/FilesProcessor.WebApi/FilesProcessor.WebApi.csproj"

dotnet watch run --project "$PROJECT" --launch-profile https