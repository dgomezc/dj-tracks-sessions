namespace DjTrackSessions.Domain.LibraryScanning;

public sealed record AudioTechnicalProperties(
    TimeSpan Duration,
    int BitrateKbps,
    int SampleRateHz,
    int BitsPerSample,
    int Channels,
    string? Codec);
