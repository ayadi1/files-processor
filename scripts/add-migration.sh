#!/usr/bin/env bash
# Creates an EF Core migration for the FilesProcessor.WebApi project.
#
# Usage:
#   ./scripts/add-migration.sh <MigrationName>
# Example:
#   ./scripts/add-migration.sh AddDownloadTickets
set -euo pipefail

MIGRATION_NAME="${1:-}"

if [[ -z "$MIGRATION_NAME" ]]; then
  echo "Usage: $0 <MigrationName>" >&2
  exit 1
fi

# Resolve repo root from the script's own location (works from any cwd).
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/src/FilesProcessor.WebApi/FilesProcessor.WebApi.csproj"

if ! command -v dotnet-ef >/dev/null 2>&1; then
  echo "dotnet-ef not found. Install it with:" >&2
  echo "  dotnet tool install --global dotnet-ef" >&2
  exit 1
fi

echo "==> Adding migration '$MIGRATION_NAME'"
dotnet ef migrations add "$MIGRATION_NAME" \
  --project "$PROJECT" \
  --startup-project "$PROJECT" \
  --output-dir Infrastructure/Migrations

echo "==> Review the generated migration under src/FilesProcessor.WebApi/Infrastructure/Migrations,"
echo "    then apply it with: dotnet ef database update --project src/FilesProcessor.WebApi"