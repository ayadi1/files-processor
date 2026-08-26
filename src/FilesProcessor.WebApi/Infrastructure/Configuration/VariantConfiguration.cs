using FilesProcessor.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FilesProcessor.WebApi.Infrastructure.Configuration;

public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.ToTable("Variants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.FileId)
            .IsRequired();

        builder.Property(v => v.FilePath)
            .IsRequired()
            .HasMaxLength(1024);

        // Store the enum as a string so the SQLite table is human-readable.
        builder.Property(v => v.Resolution)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(v => v.Width)
            .IsRequired();

        builder.Property(v => v.Height)
            .IsRequired();

        builder.Property(v => v.Size)
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        // One LocalFile has many Variants; the FK lives on the Variant side.
        builder.HasOne(v => v.File)
            .WithMany(f => f.Variants)
            .HasForeignKey(v => v.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.FileId);
    }
}
