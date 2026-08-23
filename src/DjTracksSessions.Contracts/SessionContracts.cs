namespace DjTracksSessions.Contracts;

public sealed record SessionBrowseResponse(
    string RelativePath,
    IReadOnlyList<SessionDirectoryItem> Directories,
    IReadOnlyList<SessionAudioItem> AudioFiles,
    IReadOnlyList<SessionBrowseIssue> Issues);

public sealed record SessionDirectoryItem(string Name, string RelativePath);

public sealed record SessionBrowseIssue(string RelativePath, string ErrorCode, string ErrorMessage);

public sealed record SessionAudioItem(
    string RelativePath,
    string FileName,
    string Extension,
    string? Title,
    uint? Year,
    TimeSpan? Duration,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record SessionIdentity(string RelativePath);

public sealed record SessionMetadataUpdate(
    string? Title,
    string? Artists,
    string? Album,
    uint? Year,
    string? Genre);

public sealed record SessionEditRequest(SessionIdentity Identity, SessionMetadataUpdate Update);

public sealed record SessionDetail(
    SessionIdentity Identity,
    string FileName,
    string Extension,
    string? Title,
    IReadOnlyList<string> Artists,
    string? Album,
    IReadOnlyList<string> Genres,
    uint? Year,
    string? InitialKey,
    string? BeatsPerMinute,
    TimeSpan? Duration,
    int? BitrateKbps,
    int? SampleRateHz,
    string? ErrorCode,
    string? ErrorMessage,
    SessionTracklist? Tracklist = null);

public sealed record SessionTracklist(
    string Status,
    string? SourceRelativePath,
    string? Text,
    IReadOnlyList<string> CandidateAudioPaths);

public sealed record SessionEditPreview(
    SessionIdentity Identity,
    string Format,
    SessionDetail Before,
    SessionDetail After,
    bool CanApply,
    string? FailureCode,
    string? FailureMessage);

public sealed record SessionEditResponse(SessionIdentity Identity, SessionDetail Session);
