namespace DjTracksSessions.Contracts;

public enum LibraryRoot
{
    Main,
    Pending,
    Remember
}

public sealed record TrackIdentity(LibraryRoot Root, string RelativePath);

public sealed record TrackMetadataUpdate(
    string? Title,
    string? Artists,
    string? Album,
    uint? Year,
    string? Genre,
    string? PersonalGenre);

public sealed record TrackEditRequest(TrackIdentity Identity, TrackMetadataUpdate Update);

public sealed record FilesystemTrackDetail(
    LibraryRoot Root,
    string RelativePath,
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

public sealed record TrackEditPreview(
    TrackIdentity Identity,
    string Format,
    FilesystemTrackDetail Before,
    FilesystemTrackDetail After,
    bool CanApply,
    string? FailureCode,
    string? FailureMessage);

public sealed record TrackEditResponse(TrackIdentity Identity, FilesystemTrackDetail Track);

public sealed record PendingMovePreview(
    TrackIdentity Source,
    string DestinationRelativeDirectory,
    string DestinationRelativePath,
    string FinalFileName,
    bool Collision,
    bool CanConfirm,
    string? FailureCode,
    string? FailureMessage);

public sealed record PendingMoveConfirmation(
    bool Confirmed,
    string SourceRelativePath,
    string DestinationRelativePath,
    string FinalFileName);

public sealed record PendingMoveRequest(TrackIdentity Identity, PendingMoveConfirmation Confirmation);
