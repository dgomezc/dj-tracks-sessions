namespace DjTrackSessions.Domain.LibraryScanning;

public sealed record SessionFolderDiscovery(
    string FullPath,
    string RelativePath,
    IReadOnlyList<AudioFileDiscovery> AudioFiles,
    IReadOnlyList<string> TracklistFiles,
    IReadOnlyList<string> ArtworkFiles,
    IReadOnlyList<string> Issues)
{
    public bool IsValid => Issues.Count == 0;
}
