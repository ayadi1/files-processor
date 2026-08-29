# Files Processor — Class Diagram

> Render this file in an editor/viewer that supports the **Mermaid** extension
> (e.g. VS Code *Markdown Preview Mermaid Support*, GitHub, GitLab, Notion).

```mermaid
classDiagram
    direction LR

    %% ──────────────────────────────────────────────
    %% Enum: holds the resolutions we support
    %% ──────────────────────────────────────────────
    class Resolution {
        <<enumeration>>
        Original
        Thumbnail
        Small
        Medium
        Large
        ExtraLarge
    }

    %% ──────────────────────────────────────────────
    %% Enum: file types / categories
    %% ──────────────────────────────────────────────
    class FileType {
        <<enumeration>>
        Image
        Video
        Document
        Audio
        Archive
        Other
    }

    %% ──────────────────────────────────────────────
    %% Enum: lifecycle state of a file
    %% ──────────────────────────────────────────────
    class FileStatus {
        <<enumeration>>
        Pending
        Processing
        Ready
        Failed
    }

    %% ──────────────────────────────────────────────
    %% Interface: soft-delete contract
    %% ──────────────────────────────────────────────
    class ISoftDelete {
        <<interface>>
        +DateTime? DeletedAt
        +bool IsDeleted
    }

    %% ──────────────────────────────────────────────
    %% Entity: LocalFile (an uploaded file)
    %% ──────────────────────────────────────────────
    class LocalFile {
        <<entity>>
        +Guid Id
        +string RealFileName
        +string NewFileName
        +string FilePath
        +string EncryptionKey
        +long Size
        +FileType Type
        +FileStatus Status
        +string MimeTime
        +string Extension
        +string Checksum
        +Guid UploadedBy
        +DateTime CreatedAt
        +DateTime? DeletedAt
        +bool IsDeleted
        +ICollection~Variant~ Variants
        +Create(CreateFileDto)$ LocalFile
    }

    %% ──────────────────────────────────────────────
    %% Entity: Variant (a derived/processed version of a LocalFile)
    %% ──────────────────────────────────────────────
    class Variant {
        <<entity>>
        +Guid Id
        +Guid FileId
        +string FilePath
        +Resolution Resolution
        +int Width
        +int Height
        +long Size
        +DateTime CreatedAt
        +LocalFile File
    }

    %% ──────────────────────────────────────────────
    %% Entity: DownloadTicket (short-lived download authorization)
    %% ──────────────────────────────────────────────
    class DownloadTicket {
        <<entity>>
        +Guid Id
        +Guid Token
        +DateTime CreatedAt
        +DateTime ExpiresAt
        +Guid FileId
        +Create(CreateDownloadTicketDto)$ DownloadTicket
    }

    %% ──────────────────────────────────────────────
    %% Relationships
    %% ──────────────────────────────────────────────
    ISoftDelete <|.. LocalFile : implements
    LocalFile "1" o-- "0..*" Variant : has
    DownloadTicket "0..*" --> "1" LocalFile : authorizes download of
    Variant --> Resolution : resolution
    LocalFile --> FileType : type
    LocalFile --> FileStatus : status
    Variant --> LocalFile : belongs to
```

## Notes

- **LocalFile** holds the metadata of an uploaded, encrypted file (the entity was
  renamed from `File` to `LocalFile` to avoid clashing with `System.IO.File`).
  One file can have many **Variants** (e.g. resized images / transcoded
  renditions). It implements **ISoftDelete** for logical deletion.
- **Variant** references its parent `LocalFile` via `FileId` and points to the
  physical path of the processed rendition, tagged with a `Resolution`.
- **DownloadTicket** is a short-lived, token-based authorization to download a
  file. It expires at `ExpiresAt` and points at its target `LocalFile` via
  `FileId`.
- **Resolution** is an `enum` listing the resolutions the processor supports.
- **FileType** classifies the file so the processor can pick the right pipeline
  (image resizing, video transcoding, etc.). A `FileTypeResolver` static helper
  maps content types to this enum.
- **FileStatus** tracks the async processing lifecycle: `Pending` (uploaded,
  waiting for the worker) → `Processing` → `Ready` (downloadable) or `Failed`.
- `Create(...)` static factory methods live on `LocalFile` and `DownloadTicket`
  (each entity has a private constructor to force creation through the factory).