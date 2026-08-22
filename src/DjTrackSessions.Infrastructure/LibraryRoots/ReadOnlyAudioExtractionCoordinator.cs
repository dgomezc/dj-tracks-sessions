using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Domain.LibraryScanning;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public sealed class ReadOnlyAudioExtractionCoordinator
{
    private readonly TagLibSharpAudioMetadataReader reader;

    public ReadOnlyAudioExtractionCoordinator(TagLibSharpAudioMetadataReader? reader = null) =>
        this.reader = reader ?? new TagLibSharpAudioMetadataReader();

    public AudioExtractionResult Extract(LibraryRootPolicy root, LibraryScanResult scan)
    {
        var files = scan.AudioFiles.Concat(scan.Sessions.SelectMany(session => session.AudioFiles));
        return new(files.Select(file => reader.Read(root, file)).ToArray());
    }
}
