using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class DirectLibraryBrowserTests
{
    [Fact]
    public void Browses_only_direct_children_and_exposes_corrupt_audio()
    {
        using var fixture = new BrowserFixture();
        Directory.CreateDirectory(Path.Combine(fixture.Root, "nested"));
        File.WriteAllBytes(Path.Combine(fixture.Root, "broken.mp3"), [1, 2, 3]);
        File.WriteAllBytes(Path.Combine(fixture.Root, "nested", "hidden.mp3"), [4, 5, 6]);
        File.WriteAllText(Path.Combine(fixture.Root, "notes.txt"), "not audio");

        var response = new DirectLibraryBrowser().Browse(fixture.Policy, string.Empty);

        Assert.Contains(response.Directories, directory => directory.RelativePath == "nested");
        Assert.Contains(response.AudioFiles, file => file.RelativePath == "broken.mp3" && file.ErrorCode is not null);
        Assert.DoesNotContain(response.AudioFiles, file => file.RelativePath.Contains("hidden", StringComparison.Ordinal));
        Assert.DoesNotContain(response.AudioFiles, file => file.RelativePath.EndsWith("notes.txt", StringComparison.Ordinal));
        Assert.DoesNotContain(response.AudioFiles, file => Path.IsPathFullyQualified(file.RelativePath));
    }

    [Fact]
    public void Loads_nested_folder_only_when_that_relative_path_is_requested()
    {
        using var fixture = new BrowserFixture();
        var nested = Directory.CreateDirectory(Path.Combine(fixture.Root, "2026", "House")).FullName;
        File.WriteAllBytes(Path.Combine(nested, "deep.mp3"), [1, 2, 3]);
        var browser = new DirectLibraryBrowser();

        var root = browser.Browse(fixture.Policy, string.Empty);
        var year = browser.Browse(fixture.Policy, "2026");
        var genre = browser.Browse(fixture.Policy, "2026/House");

        Assert.Contains(root.Directories, item => item.RelativePath == "2026");
        Assert.DoesNotContain(root.Directories, item => item.RelativePath == "2026/House");
        Assert.Contains(year.Directories, item => item.RelativePath == "2026/House");
        Assert.Empty(year.AudioFiles);
        Assert.Contains(genre.AudioFiles, item => item.RelativePath == "2026/House/deep.mp3");
    }

    [Fact]
    public void Rejects_traversal_and_file_paths_with_stable_errors()
    {
        using var fixture = new BrowserFixture();
        var browser = new DirectLibraryBrowser();
        File.WriteAllBytes(Path.Combine(fixture.Root, "track.mp3"), [1]);

        var traversal = Assert.Throws<FilesystemBrowseException>(() => browser.Browse(fixture.Policy, "../outside"));
        var file = Assert.Throws<FilesystemBrowseException>(() => browser.Browse(fixture.Policy, "track.mp3"));

        Assert.Equal("path.outside_root", traversal.Code);
        Assert.Equal("path.not_directory", file.Code);
    }

    [Fact]
    public void Rejects_symlink_escape_without_reading_sessions()
    {
        using var fixture = new BrowserFixture();
        using var outside = new BrowserFixture();
        File.WriteAllBytes(Path.Combine(outside.Root, "outside.mp3"), [1, 2]);
        Directory.CreateSymbolicLink(Path.Combine(fixture.Root, "escape"), outside.Root);

        var response = new DirectLibraryBrowser().Browse(fixture.Policy, string.Empty);

        Assert.DoesNotContain(response.Directories, directory => directory.Name == "escape");
        Assert.Contains(response.Issues, issue => issue.ErrorCode == "path.outside_root");
    }

    private sealed class BrowserFixture : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "dj-track-browser-" + Guid.NewGuid().ToString("N"));
        public LibraryRootPolicy Policy => new(LibraryRootType.Main, Root, LibraryRootCapability.BrowseAndPlay);

        public BrowserFixture() => Directory.CreateDirectory(Root);
        public void Dispose() => Directory.Delete(Root, recursive: true);
    }
}
