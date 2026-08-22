using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Domain.LibraryScanning;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public sealed class ReadOnlyAudioExtractionCoordinator
{
    private readonly TagLibSharpAudioMetadataReader reader;
    private readonly IncrementalAudioContentHasher hasher;

    public ReadOnlyAudioExtractionCoordinator(
        TagLibSharpAudioMetadataReader? reader = null,
        IncrementalAudioContentHasher? hasher = null) =>
        (this.reader, this.hasher) = (reader ?? new TagLibSharpAudioMetadataReader(), hasher ?? new IncrementalAudioContentHasher());

    public AudioExtractionResult Extract(LibraryRootPolicy root, LibraryScanResult scan)
    {
        var files = scan.AudioFiles.Concat(scan.Sessions.SelectMany(session => session.AudioFiles));
        return new(files.Select(file => ExtractFile(root, file)).ToArray());
    }

    private AudioExtractionOutcome ExtractFile(LibraryRootPolicy root, AudioFileDiscovery file)
    {
        var outcome = reader.Read(root, file);
        var hash = hasher.Hash(root, file);
        return hash.ContentHash is not null
            ? outcome.WithContentHash(hash.ContentHash)
            : outcome.WithHashFailure(hash.ErrorCode!, hash.ErrorMessage!);
    }
}
