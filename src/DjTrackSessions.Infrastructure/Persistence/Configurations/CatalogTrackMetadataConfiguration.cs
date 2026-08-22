using DjTrackSessions.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DjTrackSessions.Infrastructure.Persistence.Configurations;

public sealed class CatalogTrackMetadataConfiguration : IEntityTypeConfiguration<CatalogTrackMetadata>
{
    public void Configure(EntityTypeBuilder<CatalogTrackMetadata> builder)
    {
        builder.ToTable("catalog_track_metadata");
        builder.HasKey(metadata => metadata.CatalogTrackId);
        builder.Property(metadata => metadata.Title).HasMaxLength(1000);
        builder.Property(metadata => metadata.Artists).HasMaxLength(2000);
        builder.Property(metadata => metadata.Album).HasMaxLength(1000);
        builder.Property(metadata => metadata.Genre).HasMaxLength(500);
        builder.Property(metadata => metadata.PersonalGenre).HasMaxLength(100);
        builder.Property(metadata => metadata.BeatsPerMinute).HasMaxLength(50);
        builder.Property(metadata => metadata.Key).HasMaxLength(20);
    }
}
