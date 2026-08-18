namespace DjTrackSessions.Domain.LibraryScanning;

public sealed record LibraryScanResult(
    IReadOnlyList<AudioFileDiscovery> AudioFiles,
    IReadOnlyList<SessionFolderDiscovery> Sessions,
    IReadOnlyList<string> Issues);
