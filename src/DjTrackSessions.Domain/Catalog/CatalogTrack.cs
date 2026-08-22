using DjTrackSessions.Domain.LibraryRoots;

namespace DjTrackSessions.Domain.Catalog;

public sealed class CatalogTrack
{
    public int Id { get; set; }
    public LibraryRootType RootType { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string? ContentHash { get; set; }
    public long ByteLength { get; set; }
    public DateTimeOffset LastObservedAtUtc { get; set; }
    public bool IsMissing { get; set; }
    public string? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public uint Version { get; set; }
    public CatalogTrackMetadata? Metadata { get; set; }
}
