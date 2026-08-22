namespace DjTrackSessions.Domain.LibraryScanning;

public sealed record AudioExtractionOutcome(
    AudioFileDiscovery File,
    AudioTechnicalProperties? TechnicalProperties,
    CurrentAudioTags? CurrentTags,
    string? ErrorCode,
    string? ErrorMessage)
{
    public bool IsSuccess => ErrorCode is null;

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
}
