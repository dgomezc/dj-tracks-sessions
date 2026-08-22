using DjTracksSessions.Contracts;

namespace DjTracksSessions.Web.Services;

public sealed record PlaybackTrack(TrackIdentity Identity, string Title, string Artist, string FileName);

public sealed class PlaybackQueue
{
    private readonly List<PlaybackTrack> tracks = [];
    public IReadOnlyList<PlaybackTrack> Tracks => tracks;
    public int CurrentIndex { get; private set; } = -1;
    public PlaybackTrack? Current => CurrentIndex >= 0 && CurrentIndex < tracks.Count ? tracks[CurrentIndex] : null;
    public bool Add(PlaybackTrack track) { if (tracks.Any(x => x.Identity == track.Identity)) return false; tracks.Add(track); if (CurrentIndex < 0) CurrentIndex = 0; return true; }
    public bool Remove(TrackIdentity identity)
    {
        var index = tracks.FindIndex(x => x.Identity == identity);
        if (index < 0) return false;
        tracks.RemoveAt(index);
        if (tracks.Count == 0) CurrentIndex = -1;
        else if (index < CurrentIndex) CurrentIndex--;
        else if (CurrentIndex >= tracks.Count) CurrentIndex = tracks.Count - 1;
        return true;
    }
    public void Clear() { tracks.Clear(); CurrentIndex = -1; }
    public PlaybackTrack? Previous() => Select(CurrentIndex - 1);
    public PlaybackTrack? Next() => Select(CurrentIndex + 1);
    public PlaybackTrack? Select(int index) { if (index < 0 || index >= tracks.Count) return null; CurrentIndex = index; return Current; }
}
