using DjTrackSessions.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DjTrackSessions.Infrastructure.Persistence.Configurations;

public sealed class CatalogTrackConfiguration : IEntityTypeConfiguration<CatalogTrack>
{
    public void Configure(EntityTypeBuilder<CatalogTrack> builder)
    {
        builder.ToTable("catalog_tracks");
        builder.HasKey(track => track.Id);
        builder.Property(track => track.RootType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(track => track.RelativePath).HasMaxLength(2000).IsRequired();
        builder.Property(track => track.Extension).HasMaxLength(20).IsRequired();
        builder.Property(track => track.ContentHash).HasMaxLength(64);
        builder.Property(track => track.LastErrorCode).HasMaxLength(200);
        builder.Property(track => track.LastErrorMessage).HasMaxLength(2000);
        builder.Property(track => track.Version).IsRowVersion();
        builder.HasIndex(track => new { track.RootType, track.RelativePath }).IsUnique();
        builder.HasIndex(track => new { track.RootType, track.ContentHash });
        builder.HasOne(track => track.Metadata).WithOne(metadata => metadata.CatalogTrack)
            .HasForeignKey<CatalogTrackMetadata>(metadata => metadata.CatalogTrackId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
