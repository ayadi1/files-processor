using FilesProcessor.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FilesProcessor.WebApi.Infrastructure.Configuration;

public class DownloadTicketConfiguration : IEntityTypeConfiguration<DownloadTicket>
{
    public void Configure(EntityTypeBuilder<DownloadTicket> builder)
    {
        builder.ToTable("DownloadTickets");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
          .ValueGeneratedNever();

        // Tickets are looked up by token (GetTicketByToken) — the hot path.
        builder.HasIndex(f => f.Token)
          .IsUnique();

        // Cleanup of expired tickets scans by date.
        builder.HasIndex(f => f.ExpiresAt);

        // "Which tickets belong to this file?" lookups and cascade checks.
        builder.HasIndex(f => f.FileId);

        builder.HasOne<LocalFile>()
          .WithMany()
          .HasForeignKey(f => f.FileId)
          .OnDelete(DeleteBehavior.Cascade);
    }
}
