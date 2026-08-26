# Learning Roadmap

A suggested order to build this project, one small step at a time. Each step is
something for the **owner** to implement (with an AI explaining/guiding, not
writing the code). Tick them off as you go.

## Phase 0 — Clean slate

- [X] Remove the default `WeatherForecast` sample from `Program.cs`.
- [X] Set up a basic health endpoint (`GET /health`) so you can verify the app runs.

## Phase 1 — Domain model

- [X] Create a `Domain/Entities` folder (or a separate class library) for entities.
- [X] Implement the `Resolution` and `FileType` enums (see `class-diagram.md`).
- [X] Implement the `File` entity.
- [X] Implement the `Variant` entity.
- [X] Decide on the relationship + navigation properties between `File` and `Variant`.

## Phase 2 — Persistence

- [X] Choose a DB provider (start with SQLite for dev).
- [X] Add EF Core.
- [X] Create a `DbContext` with `Files` and `Variants` `DbSet`s.
- [X] Configure the entity mapping (one-to-many `File → Variants`).
- [X] Run a migration + create the database.

## Phase 3 — Upload

- [X] Define an `IFileStorage` abstraction (save / read / delete).
- [X] Implement a local-disk `IFileStorage`.
- [X] Create `POST /files` (upload) that saves the raw file + writes a `File`
      row with status `Pending`.
- [X] Return the file Id and status.

## Phase 4 — Background processing

- [ ] Pick a background-work mechanism (see `architecture.md`).
- [ ] Implement a worker that picks up `Pending` files.
- [ ] Implement the **image** pipeline: create a `Variant` per `Resolution`.
- [ ] Update file status to `Ready` (or `Failed`).

## Phase 5 — Download

- [ ] Implement `GET /files/{id}` that streams the original (or a variant) back.
- [ ] Add **short-lived download URLs** (choose a strategy).
- [ ] For non-image types: encrypt at rest, decrypt on download.

## Phase 6 — Hardening (the "lots of requests" part)

- [ ] Streaming uploads/downloads so files aren't loaded into memory.
- [ ] Bounded queue / back-pressure on the background worker.
- [ ] Rate limiting on the upload endpoint.
- [ ] Retries + dead-letter for failed jobs.
- [ ] Structured logging + metrics.

## How to use an AI for each step

Ask things like:

- "Explain how to stream a multipart upload in minimal APIs without buffering
  the whole file in memory."
- "What are the trade-offs between Channel and Hangfire for my background
  worker? Recommend one for my scale and why."
- "Review the `DbContext` I just wrote."

Avoid:

- "Write the upload endpoint for me." (You'll learn more by writing it yourself
  from the AI's explanation.)
