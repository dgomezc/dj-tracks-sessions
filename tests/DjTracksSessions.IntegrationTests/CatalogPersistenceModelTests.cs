using DjTrackSessions.Domain.Catalog;
using DjTrackSessions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class CatalogPersistenceModelTests
{
    [Fact]
    public void Catalog_model_contains_separate_track_metadata_and_session_tables()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=placeholder;Database=placeholder;Username=placeholder;Password=placeholder")
            .Options;

        using var context = new ApplicationDbContext(options);

        Assert.Equal("catalog_tracks", context.Model.FindEntityType(typeof(CatalogTrack))!.GetTableName());
        Assert.Equal("catalog_track_metadata", context.Model.FindEntityType(typeof(CatalogTrackMetadata))!.GetTableName());
        Assert.Equal("session_catalog_items", context.Model.FindEntityType(typeof(SessionCatalogItem))!.GetTableName());
        Assert.Equal("RootType", context.Model.FindEntityType(typeof(CatalogTrack))!.FindProperty(nameof(CatalogTrack.RootType))!.Name);
        Assert.NotNull(context.Model.FindEntityType(typeof(CatalogTrack))!.FindNavigation(nameof(CatalogTrack.Metadata)));
    }
}
