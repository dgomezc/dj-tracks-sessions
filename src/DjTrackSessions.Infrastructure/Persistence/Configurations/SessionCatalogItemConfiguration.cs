using DjTrackSessions.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DjTrackSessions.Infrastructure.Persistence.Configurations;

public sealed class SessionCatalogItemConfiguration : IEntityTypeConfiguration<SessionCatalogItem>
{
    public void Configure(EntityTypeBuilder<SessionCatalogItem> builder)
    {
        builder.ToTable("session_catalog_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.RelativeFolderPath).HasMaxLength(2000).IsRequired();
        builder.Property(item => item.AudioRelativePaths).HasColumnType("text[]").IsRequired();
        builder.Property(item => item.TracklistRelativePaths).HasColumnType("text[]").IsRequired();
        builder.Property(item => item.ArtworkRelativePaths).HasColumnType("text[]").IsRequired();
        builder.Property(item => item.Issues).HasColumnType("text[]").IsRequired();
        builder.HasIndex(item => item.RelativeFolderPath).IsUnique();
    }
}
