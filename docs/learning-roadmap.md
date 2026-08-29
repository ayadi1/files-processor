# Learning Roadmap

A suggested order to build this project, one small step at a time. Each step is
something for the **owner** to implement (with an AI explaining/guiding, not
writing the code). Tick them off as you go.

> Synced with the code as of 2026-08-29. Open items below are the real "next
> steps" — decided-but-unbuilt features, not placeholders.

## Phase 0 — Clean slate

- [X] Remove the default `WeatherForecast` sample from `Program.cs`.
- [X] Set up a basic health endpoint (`GET /health`) so you can verify the app runs.

## Phase 1 — Domain model

- [X] Create a `Domain/Entities` folder for entities.
- [X] Implement the `Resolution`, `FileType`, and `FileStatus` enums (see `class-diagram.md`).
- [X] Implement the `LocalFile` entity (with factory `LocalFile.Create(CreateFileDto)`).
- [X] Implement the `Variant` entity.
- [X] Decide on the relationship + navigation properties between `LocalFile` and `Variant`.

## Phase 2 — Persistence

- [X] Choose a DB provider (SQLite, configured in `AppDbContext`).
- [X] Add EF Core.
- [X] Create an `AppDbContext` with `Files` and `Variants` `DbSet`s.
- [X] Configure the entity mapping (one-to-many `LocalFile → Variants`).
- [X] Run a migration + create the database.
- [X] Soft delete: `ISoftDelete` + `AppDbContext` converting `Remove` into an
      `IsDeleted`/`DeletedAt` update.

## Phase 3 — Upload

- [X] Define an `IFileStorage` abstraction (save / read / delete / exists).
- [X] Implement a local-disk `IFileStorage` (`LocalDiskFileStorage`).
- [X] Create `POST /api/File` that saves the raw file + writes a `LocalFile`
      row with status `Pending`, returns the file Id and status.
- [X] **Stream the upload** — the controller reads multipart sections with
      `MultipartReader` instead of `IFormFile`, so nothing is buffered whole.
- [X] Compute the SHA checksum during the save (single pass over the stream).
- [X] Enforce size limits: Kestrel `MaxRequestBodySize`, `FormOptions`, and
      validation options (`UploadOptions` + `ValidateUploadOptions`).
- [X] Compensate on failure: if the DB write fails after storage succeeded,
      delete the orphaned stored file.

## Phase 4 — Background processing

- [X] Pick a background-work mechanism: **Hangfire**, behind an
      `IProcessingQueue` abstraction (`HangfireProcessingQueue`).
- [X] Implement a worker that picks up `Pending` files (`FileProcessor`).
- [X] Implement the **image** pipeline with ImageSharp: create a `Variant` per
      `Resolution`, resize with `ResizeMode.Max`.
- [X] Move the file through statuses: `Pending → Processing → Ready` (or
      `Failed`).
- [ ] Implement the **non-image** branch (currently a no-op — that's where
      encryption-at-rest goes, see Phase 6).

## Phase 5 — Application layer (CQRS refactor with MediatR)

- [X] Add MediatR and split features into `Core/Features/Files/Commands` and
      `.../Queries`.
- [X] Handlers: `UploadFile`, `GetFileById` (download), `DeleteFile`,
      `FileExists` — controller stays thin and delegates via `ISender`.
- [X] Global error handling: `GlobalExceptionHandler` + `ProblemDetails`.
- [X] Unit-test each handler with fakes (`FakeFileStorage`,
      `FakeProcessingQueue`, `ThrowingSaveContext`).

## Phase 6 — Download

- [X] Implement `GET /api/File/{id}` that streams the original back
      (`FileStreamResult` in `FileController.Download`).
- [X] Add variant downloads: stream a specific `Resolution` variant instead of
      the original (e.g. `GET /api/File/{id}?resolution=Medium`).
- [X] Add `HEAD /api/File/{id}` for cheap existence checks.
- [ ] For non-image types: encrypt at rest, decrypt on download.
- [X] Add **short-lived download URLs**.

## Phase 7 — Delete

- [X] `DELETE /api/File/{id}`: soft-delete the row, then best-effort storage
      cleanup (idempotent `DeleteAsync`; storage failure must not undo the
      soft-delete).
- [ ] Extend deletion to variants: deleting an image currently leaves the
      generated variant files on disk.

## Phase 8 — Hardening (the "lots of requests" part)

- [X] Streaming uploads (done in Phase 3 — the handler no longer buffers or
      opens the stream twice).
- [X] Structured logging (Serilog with message placeholders).
- [ ] Bounded queue / back-pressure on the background worker.
- [ ] Rate limiting on the upload endpoint (size limits exist; request-rate
      limiting does not yet).
- [ ] Retries + dead-letter for failed jobs. (Hangfire retries by default and
      the dashboard is mapped — the open part is configuring it + dead-letter
      handling / inspecting failed jobs.)
- [ ] Metrics (e.g. request duration, upload size, job success/failure counts).
- [ ] Replace hardcoded status-leak bits: `FileProcessor` still uses
      `DateTime.Now` and interpolated logging in places; standardize on UTC +
      structured templates.

## How to use an AI for each step

Ask things like:

- "Explain how the `MultipartReader` streaming in `FileController.Upload`
  works, and where back-pressure would break down at 10k concurrent uploads."
- "Review the way `DeleteFileCommandHandler` orders the DB soft-delete and the
  storage cleanup — is that the right order?"
- "What are the trade-offs for signing short-lived download URLs (HMAC token
  in the URL vs. one-time tokens in the DB)?"
- "Review the `DbContext` I just wrote."

Avoid:

- "Write the upload endpoint for me." (You'll learn more by writing it yourself
  from the AI's explanation.)
