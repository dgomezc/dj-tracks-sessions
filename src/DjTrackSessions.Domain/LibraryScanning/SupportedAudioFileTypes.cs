namespace DjTrackSessions.Domain.LibraryScanning;

public static class SupportedAudioFileTypes
{
    public static bool IsSupported(string extension) => extension.ToLowerInvariant() switch
    {
        ".mp3" or ".flac" or ".m4a" or ".aac" or ".aiff" or ".wav" => true,
        _ => false
    };

    public static bool IsSessionAudio(string extension) =>
        string.Equals(extension, ".mp3", StringComparison.OrdinalIgnoreCase);

    public static bool IsSessionArtwork(string extension) => extension.ToLowerInvariant() is ".jpg" or ".png";
}
