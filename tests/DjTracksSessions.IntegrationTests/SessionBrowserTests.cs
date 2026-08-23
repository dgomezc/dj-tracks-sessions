using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using TagLibSharp2.Id3.Id3v2;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class SessionBrowserTests
{
    [Fact]
    public void Browses_year_and_nested_session_folders_with_audio_only()
    {
        using var fixture = new Fixture();
        var folder = Directory.CreateDirectory(Path.Combine(fixture.Root, "2026", "January"));
        File.WriteAllBytes(Path.Combine(folder.FullName, "mix.mp3"), [1, 2, 3]);
        File.WriteAllText(Path.Combine(folder.FullName, "tracklist.txt"), "read only");

        var browser = new DirectSessionBrowser();
        var root = browser.Browse(fixture.Policy, string.Empty);
        var year = browser.Browse(fixture.Policy, "2026");
        var sessions = browser.Browse(fixture.Policy, "2026/January");

        Assert.Contains(root.Directories, item => item.RelativePath == "2026");
        Assert.Contains(year.Directories, item => item.RelativePath == "2026/January");
        Assert.Contains(sessions.AudioFiles, item => item.RelativePath == "2026/January/mix.mp3");
        Assert.DoesNotContain(sessions.AudioFiles, item => item.FileName == "tracklist.txt");
    }

    [Fact]
    public void Resolves_specific_tracklist_before_generic_and_keeps_source_read_only()
    {
        using var fixture = new Fixture();
        var directory = Directory.CreateDirectory(Path.Combine(fixture.Root, "2026"));
        File.WriteAllBytes(Path.Combine(directory.FullName, "mix.mp3"), [1, 2, 3]);
        File.WriteAllText(Path.Combine(directory.FullName, "tracklist.txt"), "generic");
        var specific = Path.Combine(directory.FullName, "mix.txt");
        File.WriteAllText(specific, "specific");
        var before = File.ReadAllBytes(specific);

        var detail = new DirectSessionBrowser().Detail(fixture.Policy, "2026/mix.mp3");

        Assert.Equal("resolved", detail.Tracklist?.Status);
        Assert.Equal("specific", detail.Tracklist?.Text);
        Assert.Equal(before, File.ReadAllBytes(specific));
    }

    [Fact]
    public void Marks_generic_tracklist_ambiguous_when_multiple_audios_share_it()
    {
        using var fixture = new Fixture();
        File.WriteAllBytes(Path.Combine(fixture.Root, "one.mp3"), [1]);
        File.WriteAllBytes(Path.Combine(fixture.Root, "two.mp3"), [2]);
        File.WriteAllText(Path.Combine(fixture.Root, "tracklist.txt"), "shared");

        var detail = new DirectSessionBrowser().Detail(fixture.Policy, "one.mp3");

        Assert.Equal("ambiguous", detail.Tracklist?.Status);
        Assert.Equal(2, detail.Tracklist?.CandidateAudioPaths.Count);
        Assert.Null(detail.Tracklist?.Text);
    }

    [Fact]
    public void Reports_missing_and_invalid_tracklists_without_mutating_source()
    {
        using var fixture = new Fixture();
        File.WriteAllBytes(Path.Combine(fixture.Root, "mix.mp3"), [1]);
        var specific = Path.Combine(fixture.Root, "mix.txt");

        Assert.Equal("missing", new DirectSessionBrowser().Detail(fixture.Policy, "mix.mp3").Tracklist?.Status);
        File.WriteAllBytes(specific, [0xC3, 0x28]);
        var before = File.ReadAllBytes(specific);
        var detail = new DirectSessionBrowser().Detail(fixture.Policy, "mix.mp3");

        Assert.Equal("invalid", detail.Tracklist?.Status);
        Assert.Equal(before, File.ReadAllBytes(specific));
    }

    [Fact]
    public void Rejects_traversal_and_symlink_escape()
    {
        using var fixture = new Fixture();
        using var outside = new Fixture();
        Directory.CreateSymbolicLink(Path.Combine(fixture.Root, "escape"), outside.Root);
        var browser = new DirectSessionBrowser();

        var traversal = Assert.Throws<SessionBrowseException>(() => browser.Browse(fixture.Policy, "../outside"));
        var response = browser.Browse(fixture.Policy, string.Empty);

        Assert.Equal("path.outside_root", traversal.Code);
        Assert.Contains(response.Issues, issue => issue.ErrorCode == "path.outside_root");
    }

    [Fact]
    public async Task Session_metadata_edit_previews_verifies_and_replaces_only_the_confined_mp3()
    {
        using var fixture = new Fixture();
        var relativePath = "2026/mix.mp3";
        var path = Path.Combine(fixture.Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, [.. new Id3v2Tag { Title = "Before", Artist = "Old Artist" }.Render(), .. Enumerable.Repeat((byte)0, 4096)]);
        var writer = new SessionMetadataWriter();
        var request = new DjTracksSessions.Contracts.SessionMetadataUpdate("After", "Artist One, Artist Two", "Album", 2026, "House");

        var preview = await writer.PreviewAsync(fixture.Policy, new(relativePath), request, CancellationToken.None);
        var result = await writer.ApplyAsync(fixture.Policy, new(relativePath), request, CancellationToken.None);
        var detail = new DirectSessionBrowser().Detail(fixture.Policy, relativePath);

        Assert.True(preview.IsSuccess);
        Assert.Equal("Before", preview.Value.Before.Title);
        Assert.Equal("After", preview.Value.After.Title);
        Assert.True(result.IsSuccess);
        Assert.Equal("After", detail.Title);
        Assert.Equal(new[] { "Artist One", "Artist Two" }, detail.Artists);
        Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(path)!, "*.dj-tmp-*"));
    }

    [Fact]
    public async Task Session_metadata_edit_rejects_unsupported_formats_without_mutation()
    {
        using var fixture = new Fixture();
        var path = Path.Combine(fixture.Root, "mix.flac");
        File.WriteAllBytes(path, [1, 2, 3]);
        var before = File.ReadAllBytes(path);
        var result = await new SessionMetadataWriter().PreviewAsync(fixture.Policy, new("mix.flac"), new("After", null, null, null, null), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal("tag.unsupported", result.Errors[0].Metadata["errorCode"]);
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "dj-session-browser-" + Guid.NewGuid().ToString("N"));
        public LibraryRootPolicy Policy => new(LibraryRootType.Sessions, Root, LibraryRootCapability.SeparateSessionCollection);
        public Fixture() => Directory.CreateDirectory(Root);
        public void Dispose() => Directory.Delete(Root, true);
    }
}
