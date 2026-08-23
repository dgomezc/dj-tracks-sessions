using DjTracksSessions.Contracts;

namespace DjTracksSessions.Web.Services;

public enum PlaybackStatus { Empty, Loading, Playing, Paused, Ended, Unavailable, Error }
public enum AudioCommandKind { Load, Play, Pause, Seek, Volume }
public sealed record AudioCommand(AudioCommandKind Kind, string? StreamUrl = null, double Value = 0);

public sealed class PlaybackService
{
    private readonly Uri apiBaseUrl;

    public PlaybackService(IConfiguration configuration)
    {
        var configuredBaseUrl = configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException("Api:BaseUrl must be configured.");
        apiBaseUrl = new Uri(configuredBaseUrl.EndsWith('/') ? configuredBaseUrl : configuredBaseUrl + '/', UriKind.Absolute);
    }

    public PlaybackQueue Queue { get; } = new();
    public PlaybackStatus Status { get; private set; } = PlaybackStatus.Empty;
    public bool IsPlaying => Status == PlaybackStatus.Playing;
    public double Volume { get; private set; } = 1;
    public double Position { get; private set; }
    public double Duration { get; private set; }
    public string? ErrorMessage { get; private set; }
    public event Action? Changed;
    public event Action<AudioCommand>? AudioCommandRequested;
    public void Add(LibraryRoot root, FilesystemAudioItem track, bool play = false)
    {
        Add(new PlaybackTrack(new TrackIdentity(root, track.RelativePath), track.Title ?? track.FileName, track.Artists.Count == 0 ? "Artista desconocido" : string.Join(", ", track.Artists), track.FileName), play);
    }
    public void Add(SessionAudioItem session, bool play = false) =>
        Add(new PlaybackTrack(new TrackIdentity(LibraryRoot.Sessions, session.RelativePath), session.Title ?? session.FileName, "Sesión", session.FileName), play);
    private void Add(PlaybackTrack item, bool play)
    {
        Queue.Add(item);
        if (play) Play(item); else Changed?.Invoke();
    }
    public void Remove(TrackIdentity identity) { Queue.Remove(identity); if (Queue.Current is null) Stop(); else Changed?.Invoke(); }
    public void Clear() { Queue.Clear(); Stop(); }
    public void Previous() { if (Queue.Previous() is { } track) Play(track); }
    public void Next() { if (Queue.Next() is { } track) Play(track); else SetStatus(PlaybackStatus.Ended); }
    public void TogglePlayPause() { if (Queue.Current is null) return; if (IsPlaying) { SetStatus(PlaybackStatus.Paused); Request(new(AudioCommandKind.Pause)); } else if (Status == PlaybackStatus.Paused) { SetStatus(PlaybackStatus.Playing); Request(new(AudioCommandKind.Play)); } else Play(Queue.Current); }
    public void Seek(double seconds) { Position = Math.Max(0, seconds); Request(new(AudioCommandKind.Seek, Value: Position)); Changed?.Invoke(); }
    public void SetVolume(double volume) { Volume = Math.Clamp(volume, 0, 1); Request(new(AudioCommandKind.Volume, Value: Volume)); Changed?.Invoke(); }
    public void NotifyLoaded(double duration) { Duration = duration; ErrorMessage = null; SetStatus(PlaybackStatus.Playing); }
    public void NotifyTime(double position, double duration) { Position = position; Duration = duration; Changed?.Invoke(); }
    public void NotifyPaused() => SetStatus(PlaybackStatus.Paused);
    public void NotifyEnded() { Position = Duration; SetStatus(PlaybackStatus.Ended); }
    public void NotifyUnavailable(string message) { ErrorMessage = message; SetStatus(PlaybackStatus.Unavailable); }
    public void NotifyError(string message) { ErrorMessage = message; SetStatus(PlaybackStatus.Error); }
    private void Play(PlaybackTrack track) { Queue.Select(Queue.Tracks.ToList().IndexOf(track)); Position = 0; Duration = 0; ErrorMessage = null; SetStatus(PlaybackStatus.Loading); Request(new(AudioCommandKind.Load, BuildStreamUrl(track.Identity))); }
    internal string BuildStreamUrl(TrackIdentity identity)
    {
        var streamUrl = new Uri(apiBaseUrl, "playback/stream");
        return $"{streamUrl}?root={Uri.EscapeDataString(identity.Root.ToString())}&path={Uri.EscapeDataString(identity.RelativePath)}";
    }
    private void Stop() { Position = 0; Duration = 0; ErrorMessage = null; SetStatus(Queue.Current is null ? PlaybackStatus.Empty : PlaybackStatus.Paused); }
    private void SetStatus(PlaybackStatus status) { Status = status; Changed?.Invoke(); }
    private void Request(AudioCommand command) => AudioCommandRequested?.Invoke(command);
}
