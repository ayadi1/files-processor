# Architecture & Feature Plan

This document describes the **intended** architecture and feature set. It is a
guide for the owner to build from — not a spec for an AI to implement. Items
marked **(open)** are decisions the owner still needs to make.

## High-level flow

```
Client ──upload──▶ Web API ──persist metadata + save raw file──▶ Storage
                       │
                       └──enqueue "process this file"──▶ Background worker
                                                            │
                                                            ▼
                                                   Process by FileType
                                                   (image → variants,
                                                    other → encrypt/…)
                                                            │
                                                            ▼
                                                   Persist variants /
                                                   mark file ready
                                                            │
                                                            ▼
Client ◀──download / short-lived URL── Web API ◀─ read variants / decrypt
```

## Components (planned)

### 1. Upload endpoint

- Accepts a file (multipart/form-data or streaming).
- Generates a `File` record (Id, names, path, size, type, checksum, …).
- Saves the raw file to storage.
- Enqueues a background processing job.
- Returns immediately with the file Id + status `Pending`.

### 2. Background processor  (open: mechanism)

Options to discuss with the owner:

- `IHostedService` + `BackgroundService` (in-process).
- `System.Threading.Channels` for an in-memory queue.
- A real queue (e.g. Azure Service Bus, RabbitMQ) + a worker service.
- A library (e.g. Hangfire, Quartz).

### 3. Processing pipelines (selected by `FileType`)

- **Image** → produce `Variant`s at each `Resolution` (resize). Library (open):
  ImageSharp, SkiaSharp, Magick.NET, System.Drawing…
- **Other types** → encryption at rest (encrypt the stored file with the
  `EncryptionKey`), and/or prepare for short-lived download URLs.

### 4. Download endpoint

- Returns a file by Id, decrypting on the fly if needed.
- **Or** issues a **short-lived (expiring) download URL** (open: signed URL
  from a blob store, or an in-app token with TTL).

### 5. Persistence  (open: store)

- EF Core + a database (SQL Server / PostgreSQL / SQLite for dev).
- Tables mirror the domain model (`Files`, `Variants`).

### 6. Storage  (open: where files live)

- Local disk for dev → abstract behind an `IFileStorage` interface so it can
  swap to a blob store (S3 / Azure Blob / GCS) later.

## Cross-cutting concerns to think about

- **Concurrency / high volume:** backpressure on uploads, bounded queues,
  rate limiting, async streaming to avoid loading files into memory.
- **Security:** encryption keys management (open: where stored, how rotated),
  authorization on upload/download, validation of file types & size limits.
- **Reliability:** retries for processing jobs, idempotency, dead-letter for
  failed jobs, status tracking (`Pending` / `Processing` / `Ready` / `Failed`).
- **Observability:** logging, structured logs, metrics, tracing.

## Open decisions checklist

- [ ] Background-work mechanism
- [ ] Image processing library
- [ ] Persistence (DB + provider)
- [ ] File storage (disk vs blob) and the storage abstraction
- [ ] Encryption key management
- [ ] Short-lived URL strategy (signed blob URL vs in-app token)
- [ ] Status model for a file's processing lifecycle
