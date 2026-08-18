using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class ReadOnlyLibraryScannerTests
{
    [Fact]
    public void Scan_ordinary_root_discovers_supported_audio_recursively_without_reading_tags()
    {
        using var fixture = ScanFixture.Create();
        var nested = Directory.CreateDirectory(Path.Combine(fixture.Root, "nested")).FullName;
        foreach (var extension in new[] { ".mp3", ".flac", ".m4a", ".aac", ".aiff", ".wav" })
        {
            File.WriteAllText(Path.Combine(nested, $"track{extension}"), "fixture");
        }
        File.WriteAllText(Path.Combine(nested, "ignored.txt"), "unchanged");
        var before = File.ReadAllText(Path.Combine(nested, "ignored.txt"));

        var result = new ReadOnlyLibraryScanner().Scan(fixture.Policy(LibraryRootType.Main));

        Assert.Equal(6, result.AudioFiles.Count);
        Assert.Empty(result.Sessions);
        Assert.Equal(before, File.ReadAllText(Path.Combine(nested, "ignored.txt")));
    }

    [Fact]
    public void Scan_sessions_requires_mp3_and_txt_and_keeps_artwork_optional()
    {
        using var fixture = ScanFixture.Create();
        var valid = Directory.CreateDirectory(Path.Combine(fixture.Root, "valid")).FullName;
        File.WriteAllText(Path.Combine(valid, "session.mp3"), "audio");
        File.WriteAllText(Path.Combine(valid, "tracklist.txt"), "1. Track");
        File.WriteAllText(Path.Combine(valid, "cover.JPG"), "image");
        var invalid = Directory.CreateDirectory(Path.Combine(fixture.Root, "invalid")).FullName;
        File.WriteAllText(Path.Combine(invalid, "session.mp3"), "audio");

        var result = new ReadOnlyLibraryScanner().Scan(fixture.Policy(LibraryRootType.Sessions));

        var validSession = Assert.Single(result.Sessions, session => session.RelativePath == "valid");
        Assert.True(validSession.IsValid);
        Assert.Single(validSession.ArtworkFiles);
        var invalidSession = Assert.Single(result.Sessions, session => session.RelativePath == "invalid");
        Assert.Contains("session.tracklist_missing", invalidSession.Issues);
    }

    [Fact]
    public void Scan_rejects_symlink_escape_and_does_not_discover_outside_file()
    {
        using var fixture = ScanFixture.Create();
        var outside = Directory.CreateDirectory(Path.Combine(fixture.Parent, "outside")).FullName;
        File.WriteAllText(Path.Combine(outside, "escaped.mp3"), "outside");
        Directory.CreateSymbolicLink(Path.Combine(fixture.Root, "escape"), outside);

        var result = new ReadOnlyLibraryScanner().Scan(fixture.Policy(LibraryRootType.Main));

        Assert.DoesNotContain(result.AudioFiles, file => file.FullPath.Contains("escaped.mp3", StringComparison.Ordinal));
        Assert.Contains(result.Issues, issue => issue.Contains("escape", StringComparison.Ordinal));
    }

    private sealed class ScanFixture : IDisposable
    {
        private ScanFixture(string parent, string root) => (Parent, Root) = (parent, root);

        public string Parent { get; }
        public string Root { get; }

        public static ScanFixture Create()
        {
            var parent = Directory.CreateTempSubdirectory("dj-scan-").FullName;
            return new(parent, Directory.CreateDirectory(Path.Combine(parent, "root")).FullName);
        }

        public LibraryRootPolicy Policy(LibraryRootType type) =>
            new(type, Path.GetFullPath(Root), LibraryRootCapability.IndexAutomatically);

        public void Dispose() => Directory.Delete(Parent, true);
    }
}
