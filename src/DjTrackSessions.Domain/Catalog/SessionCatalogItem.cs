namespace DjTrackSessions.Domain.Catalog;

public sealed class SessionCatalogItem
{
    public int Id { get; set; }
    public string RelativeFolderPath { get; set; } = string.Empty;
    public string[] AudioRelativePaths { get; set; } = [];
    public string[] TracklistRelativePaths { get; set; } = [];
    public string[] ArtworkRelativePaths { get; set; } = [];
    public TimeSpan? Duration { get; set; }
    public DateTimeOffset LastObservedAtUtc { get; set; }
    public bool IsValid { get; set; }
    public string[] Issues { get; set; } = [];
}
