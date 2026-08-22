namespace DjTrackSessions.Domain.LibraryScanning;

public sealed record AudioExtractionResult(IReadOnlyList<AudioExtractionOutcome> Outcomes);
