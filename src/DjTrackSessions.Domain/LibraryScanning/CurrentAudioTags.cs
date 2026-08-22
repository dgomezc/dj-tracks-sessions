namespace DjTrackSessions.Domain.LibraryScanning;

public sealed record CurrentAudioTags(
    string? Title,
    IReadOnlyList<string> Artists,
    string? Album,
    IReadOnlyList<string> AlbumArtists,
    IReadOnlyList<string> Genres,
    uint? Year,
    uint? TrackNumber,
    uint? TrackCount,
    uint? DiscNumber,
    uint? DiscCount,
    string? Comment,
    string? Composer,
    string? Isrc,
    string? InitialKey,
    string? BeatsPerMinute,
    string? PersonalGenre = null);
