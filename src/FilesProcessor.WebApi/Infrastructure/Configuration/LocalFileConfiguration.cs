using FilesProcessor.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FilesProcessor.WebApi.Infrastructure.Configuration
{
  public class LocalFileConfiguration : IEntityTypeConfiguration<LocalFile>
  {
    public void Configure(EntityTypeBuilder<LocalFile> builder)
    {
      builder.ToTable("Files");

      builder.HasKey(f => f.Id);

      builder.Property(f => f.Id)
        .ValueGeneratedNever();

      builder.Property(f => f.RealFileName)
        .IsRequired()
        .HasMaxLength(512);

      builder.Property(f => f.NewFileName)
        .IsRequired()
        .HasMaxLength(512);

      builder.Property(f => f.FilePath)
        .IsRequired()
        .HasMaxLength(1024);

      builder.Property(f => f.EncryptionKey)
        .IsRequired()
        .HasMaxLength(512);

      builder.Property(f => f.Size)
        .IsRequired();

      // Store the enum as a string so the SQLite table is human-readable.
      builder.Property(f => f.Type)
        .HasConversion<string>()
        .HasMaxLength(32)
        .IsRequired();

      builder.Property(f => f.MimeTime)
        .IsRequired()
        .HasMaxLength(128);

      builder.Property(f => f.Extension)
        .IsRequired()
        .HasMaxLength(16);

      builder.Property(f => f.Checksum)
        .IsRequired()
        .HasMaxLength(128);

      builder.Property(f => f.UploadedBy)
        .IsRequired();

      builder.Property(f => f.CreatedAt)
        .IsRequired();

      builder.Property(f => f.DeletedAt);

      builder.Property(f => f.IsDeleted)
        .IsRequired();

      builder.Property(f => f.Status)
        .HasConversion<string>()
        .HasMaxLength(32)
        .IsRequired();

      // Speed up queries that filter out soft-deleted rows.
      builder.HasIndex(f => f.IsDeleted);
      builder.HasIndex(f => f.UploadedBy);

      // manage soft delete
      builder.HasQueryFilter(f => !f.IsDeleted);
    }
  }
}