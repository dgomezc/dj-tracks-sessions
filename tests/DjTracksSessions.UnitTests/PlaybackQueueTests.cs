using DjTracksSessions.Contracts;
using DjTracksSessions.Web.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DjTracksSessions.UnitTests;

public sealed class PlaybackQueueTests
{
    private static PlaybackTrack Track(string path) => new(new TrackIdentity(LibraryRoot.Main, path), path, "Artist", path);

    [Fact]
    public void Add_remove_clear_and_navigation_keep_current_item_consistent()
    {
        var queue = new PlaybackQueue();
        var first = Track("one.mp3");
        var second = Track("two.mp3");
        var third = Track("three.mp3");

        Assert.True(queue.Add(first));
        Assert.True(queue.Add(second));
        Assert.True(queue.Add(third));
        Assert.Same(first, queue.Current);
        Assert.Same(second, queue.Next());
        Assert.Same(first, queue.Previous());
        Assert.True(queue.Remove(first.Identity));
        Assert.Same(second, queue.Current);
        queue.Clear();
        Assert.Null(queue.Current);
        Assert.Empty(queue.Tracks);
    }

    [Fact]
    public void Duplicate_files_are_not_added_and_next_ends_at_queue_boundary()
    {
        var queue = new PlaybackQueue();
        var track = Track("same.mp3");
        Assert.True(queue.Add(track));
        Assert.False(queue.Add(track));
        Assert.Null(queue.Next());
    }

    [Fact]
    public void Playback_uses_absolute_api_stream_url_and_encodes_root_path_query_values()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Api:BaseUrl"] = "http://api.example.test/base/"
        }).Build();
        var service = new PlaybackService(configuration);
        AudioCommand? command = null;
        service.AudioCommandRequested += requested => command = requested;

        service.Add(LibraryRoot.Main, new FilesystemAudioItem(
            RelativePath: "sets/House & Test.mp3",
            FileName: "House & Test.mp3",
            Extension: ".mp3",
            Title: "Track",
            Artists: ["Artist"],
            Album: null,
            Genres: [],
            Year: null,
            PersonalGenre: null,
            InitialKey: null,
            BeatsPerMinute: null,
            Duration: null,
            BitrateKbps: null,
            SampleRateHz: null,
            ErrorCode: null,
            ErrorMessage: null), true);

        Assert.Equal("http://api.example.test/base/playback/stream?root=Main&path=sets%2FHouse%20%26%20Test.mp3", command?.StreamUrl);
    }

    [Fact]
    public void Browser_error_notifications_are_exposed_as_visible_playback_states()
    {
        var service = new PlaybackService(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Api:BaseUrl"] = "http://api.example.test"
        }).Build());

        service.NotifyUnavailable("El audio dejó de estar disponible.");
        Assert.Equal(PlaybackStatus.Unavailable, service.Status);
        Assert.Equal("El audio dejó de estar disponible.", service.ErrorMessage);

        service.NotifyError("El navegador no pudo reproducir este audio.");
        Assert.Equal(PlaybackStatus.Error, service.Status);
        Assert.Equal("El navegador no pudo reproducir este audio.", service.ErrorMessage);
    }

    [Fact]
    public void Session_audio_is_mapped_to_the_global_playback_identity_and_stream()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Api:BaseUrl"] = "http://api.example.test/base/"
        }).Build();
        var service = new PlaybackService(configuration);
        AudioCommand? command = null;
        service.AudioCommandRequested += requested => command = requested;

        service.Add(new SessionAudioItem("2026/Set & One.mp3", "Set & One.mp3", ".mp3", "Session", null, null, null, null), true);

        Assert.Equal(new TrackIdentity(LibraryRoot.Sessions, "2026/Set & One.mp3"), service.Queue.Current?.Identity);
        Assert.Equal("http://api.example.test/base/playback/stream?root=Sessions&path=2026%2FSet%20%26%20One.mp3", command?.StreamUrl);
    }
}
