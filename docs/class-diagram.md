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
    %% Entity: File
    %% ──────────────────────────────────────────────
    class File {
        +Guid Id
        +string RealFileName
        +string NewFileName
        +string FilePath
        +string EncryptionKey
        +long Size
        +FileType Type
        +string MimeType
        +string Extension
        +string Checksum
        +Guid? UploadedBy
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        +DateTime? DeletedAt
        +bool IsDeleted
        +ICollection~Variant~ Variants
    }

    %% ──────────────────────────────────────────────
    %% Entity: Variant (a derived/processed version of a File)
    %% ──────────────────────────────────────────────
    class Variant {
        +Guid Id
        +Guid FileId
        +string FilePath
        +Resolution Resolution
        +int Width
        +int Height
        +long Size
        +DateTime CreatedAt
        +File File
    }

    %% ──────────────────────────────────────────────
    %% Relationships
    %% ──────────────────────────────────────────────
    File "1" o-- "0..*" Variant : has
    File --> FileType : type
    Variant --> Resolution : resolution
    Variant --> File : belongs to
```

## Notes

- **File** holds the metadata of an uploaded, encrypted file. One file can have
  many **Variants** (e.g. resized images / transcoded renditions).
- **Variant** references its parent `File` via `FileId` and points to the
  physical path of the processed rendition, tagged with a `Resolution`.
- **Resolution** is an `enum` listing the resolutions the processor supports.
- **FileType** is a small extra enum that classifies the file so the processor
  can pick the right pipeline (image resizing, video transcoding, etc.).