# Files Processor

A .NET 10 Web API for handling **high-volume file upload/download** with
**asynchronous background processing** — creating multiple renditions
(resized variants) for images, and encryption/decryption + short-lived download
URLs for other file types.

> ⚠️ **This is a learning project.** The owner is building it themselves to
> learn. See [`CLAUDE.md`](./CLAUDE.md) for the rules any AI assistant must
> follow when helping here — most importantly: **do not write the implementation
> code; teach and guide instead.**

---

## Vision

A backend that can:

1. **Accept many concurrent uploads** — users upload a file through the API.
2. **Kick off async processing** — the upload returns immediately with an
   acknowledgement; the heavy work happens in the background.
3. **Produce variants** depending on file type:
   - **Images** → generate different resolutions / sizes (variants).
   - **Other types** (documents, video, audio, archives) → other pipelines
     such as **encryption at rest** and **decryption on download**, or
     generating **short-lived (expiring) download URLs**.
4. **Serve downloads** — either stream the file back (decrypted on the fly) or
   hand the client a temporary, time-limited URL.

---

## Tech stack (current & planned)

| Area            | Choice                                   |
| --------------- | ---------------------------------------- |
| Language        | C# / .NET 10                             |
| Web framework   | ASP.NET Core Web API Controllers.        |
| API description | OpenAPI (`Microsoft.AspNetCore.OpenApi`) |
| Storage         | Disk (local disk → blob store).          |
| Background work | Hangfire (hosted services / queue).       |
| Persistence     | SQLite.                                  |

Anything marked _to be decided_ is a learning decision the owner will make,
not something for an AI to fill in.

---

## Repository layout

```
files-processor/
├── docs/                  # Documentation & diagrams
│   ├── class-diagram.md   # Mermaid class diagram (File, Variant, enums)
│   ├── architecture.md    # Planned architecture & feature breakdown
│   └── learning-roadmap.md# Step-by-step learning path
├── src/
│   └── FilesProcessor.WebApi/   # The Web API project (currently the template)
└── CLAUDE.md              # Rules for AI assistants working in this repo
```

---

## Domain model (summary)

See [`docs/class-diagram.md`](./docs/class-diagram.md) for the full Mermaid
diagram.

- **`File`** — metadata for an uploaded file: `Id`, `RealFileName`,
  `NewFileName`, `FilePath`, `EncryptionKey`, `Size`, `Type`, …
- **`Variant`** — a processed rendition of a file: `Id`, `FileId`, `FilePath`,
  `Resolution`, width/height, size.
- **`Resolution`** (enum) — the resolutions we support
  (`Thumbnail`, `Small`, `Medium`, `Large`, …).
- **`FileType`** (enum) — `Image`, `Video`, `Document`, `Audio`, `Archive`,
  `Other` — used to route a file to the right processing pipeline.

---

## Getting started

```bash
cd src/FilesProcessor.WebApi
dotnet run
```

The API currently exposes the default template endpoint only. Real endpoints
will be added by the owner as the project grows.

---

## Status

Early stage — the Web API project exists with the default ASP.NET Core template.
Domain entities, upload/download endpoints, and background processing are the
next things to build (see the [learning roadmap](./docs/learning-roadmap.md)).
