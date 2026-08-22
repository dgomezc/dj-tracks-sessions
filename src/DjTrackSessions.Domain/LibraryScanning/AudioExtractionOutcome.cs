namespace DjTrackSessions.Domain.LibraryScanning;

public sealed record AudioExtractionOutcome(
    AudioFileDiscovery File,
    AudioTechnicalProperties? TechnicalProperties,
    CurrentAudioTags? CurrentTags,
    string? ErrorCode,
    string? ErrorMessage,
    string? ContentHash = null,
    string? HashErrorCode = null,
    string? HashErrorMessage = null)
{
    public bool IsSuccess => ErrorCode is null;
    public bool IsHashSuccess => ContentHash is not null && HashErrorCode is null;

    public static AudioExtractionOutcome Success(
        AudioFileDiscovery file,
        AudioTechnicalProperties technicalProperties,
        CurrentAudioTags currentTags) =>
        new(file, technicalProperties, currentTags, null, null);

    public static AudioExtractionOutcome Failure(
        AudioFileDiscovery file,
        string errorCode,
        string errorMessage) =>
        new(file, null, null, errorCode, errorMessage);

    public AudioExtractionOutcome WithContentHash(string contentHash) =>
        this with { ContentHash = contentHash, HashErrorCode = null, HashErrorMessage = null };

    public AudioExtractionOutcome WithHashFailure(string errorCode, string errorMessage) =>
        this with { HashErrorCode = errorCode, HashErrorMessage = errorMessage };
}
