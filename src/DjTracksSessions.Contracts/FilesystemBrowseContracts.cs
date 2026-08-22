namespace DjTracksSessions.Contracts;

public sealed record FilesystemBrowseResponse(
    LibraryRoot Root,
    string RelativePath,
    IReadOnlyList<FilesystemDirectoryItem> Directories,
    IReadOnlyList<FilesystemAudioItem> AudioFiles,
    IReadOnlyList<FilesystemBrowseIssue> Issues);

public sealed record FilesystemDirectoryItem(string Name, string RelativePath);

public sealed record FilesystemBrowseIssue(string RelativePath, string ErrorCode, string ErrorMessage);

public sealed record FilesystemAudioItem(
    string RelativePath,
    string FileName,
    string Extension,
    string? Title,
    IReadOnlyList<string> Artists,
    string? Album,
    IReadOnlyList<string> Genres,
    uint? Year,
    string? PersonalGenre,
    string? InitialKey,
    string? BeatsPerMinute,
    TimeSpan? Duration,
    int? BitrateKbps,
    int? SampleRateHz,
    string? ErrorCode,
    string? ErrorMessage);
