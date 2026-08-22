namespace DjTrackSessions.Domain.Catalog;

public sealed class CatalogTrackMetadata
{
    public int CatalogTrackId { get; set; }
    public string? Title { get; set; }
    public string? Artists { get; set; }
    public string? Album { get; set; }
    public uint? Year { get; set; }
    public string? Genre { get; set; }
    public string? PersonalGenre { get; set; }
    public string? BeatsPerMinute { get; set; }
    public string? Key { get; set; }
    public TimeSpan? Duration { get; set; }
    public int? BitrateKbps { get; set; }
    public int? SampleRateHz { get; set; }
    public CatalogTrack CatalogTrack { get; set; } = null!;
}
