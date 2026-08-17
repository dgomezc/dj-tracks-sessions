using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class LibraryRootConfigurationTests
{
    [Fact]
    public void Load_configures_four_canonical_roots_and_policies()
    {
        using var fixture = RootFixture.Create();

        var result = LibraryRootConfiguration.Load(fixture.Configuration);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.Count);
        Assert.Equal(
            [LibraryRootType.Main, LibraryRootType.Pending, LibraryRootType.Remember, LibraryRootType.Sessions],
            result.Value.Select(root => root.Type));
        Assert.Equal(Path.GetFullPath(fixture.Values["MAIN_LIBRARY_PATH"]!), result.Value[0].CanonicalPath);
        Assert.True(result.Value.Single(root => root.Type == LibraryRootType.Remember).Capabilities.HasFlag(LibraryRootCapability.ForceRememberGenre));
        Assert.True(result.Value.Single(root => root.Type == LibraryRootType.Sessions).Capabilities.HasFlag(LibraryRootCapability.SeparateSessionCollection));
    }

    [Theory]
    [InlineData("PENDING_LIBRARY_PATH", "root.missing")]
    [InlineData("REMEMBER_LIBRARY_PATH", "root.invalid_path")]
    public void Load_rejects_missing_or_nonexistent_roots(string key, string errorCode)
    {
        using var fixture = RootFixture.Create();
        fixture.Configuration[key] = key == "PENDING_LIBRARY_PATH" ? " " : Path.Combine(fixture.Directory, "missing");

        var result = LibraryRootConfiguration.Load(fixture.Configuration);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error =>
            error.Metadata.TryGetValue("errorCode", out var actualCode) &&
            string.Equals(actualCode?.ToString(), errorCode, StringComparison.Ordinal));
    }

    [Fact]
    public void Load_rejects_overlapping_roots_after_canonicalization()
    {
        using var fixture = RootFixture.Create();
        fixture.Configuration["PENDING_LIBRARY_PATH"] = fixture.Values["MAIN_LIBRARY_PATH"];

        var result = LibraryRootConfiguration.Load(fixture.Configuration);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error =>
            string.Equals(error.Metadata["errorCode"]?.ToString(), "root.overlap", StringComparison.Ordinal));
    }

    [Fact]
    public void Load_resolves_a_root_symlink()
    {
        using var fixture = RootFixture.Create();
        var target = Path.Combine(fixture.Directory, "remember-target");
        var link = Path.Combine(fixture.Directory, "remember-link");
        Directory.CreateDirectory(target);
        Directory.Delete(fixture.Values["REMEMBER_LIBRARY_PATH"]!, true);
        Directory.CreateSymbolicLink(link, target);
        fixture.Configuration["REMEMBER_LIBRARY_PATH"] = link;

        var result = LibraryRootConfiguration.Load(fixture.Configuration);

        Assert.True(result.IsSuccess);
        Assert.Equal(Path.GetFullPath(target), result.Value.Single(root => root.Type == LibraryRootType.Remember).CanonicalPath);
    }

    [Fact]
    public void ResolveExistingPath_rejects_traversal_and_symlink_escape()
    {
        using var fixture = RootFixture.Create();
        var outside = Path.Combine(fixture.Directory, "outside");
        var link = Path.Combine(fixture.Values["MAIN_LIBRARY_PATH"]!, "outside-link");
        Directory.CreateDirectory(outside);
        Directory.CreateSymbolicLink(link, outside);
        var root = LibraryRootConfiguration.Load(fixture.Configuration).Value.Single(root => root.Type == LibraryRootType.Main);

        var traversal = LibraryRootPathResolver.ResolveExistingPath(root, "../pending");
        var escapedLink = LibraryRootPathResolver.ResolveExistingPath(root, "outside-link");

        Assert.False(traversal.IsSuccess);
        Assert.False(escapedLink.IsSuccess);
        Assert.All(new[] { traversal, escapedLink }, result =>
            Assert.Contains(result.Errors, error =>
                string.Equals(error.Metadata["errorCode"]?.ToString(), "path.outside_root", StringComparison.Ordinal)));
    }

    [Fact]
    public void ResolveExistingPath_returns_existing_child_and_rejects_missing_child()
    {
        using var fixture = RootFixture.Create();
        var file = Path.Combine(fixture.Values["MAIN_LIBRARY_PATH"]!, "track.mp3");
        File.WriteAllText(file, "fixture");
        var root = LibraryRootConfiguration.Load(fixture.Configuration).Value.Single(root => root.Type == LibraryRootType.Main);

        var existing = LibraryRootPathResolver.ResolveExistingPath(root, "track.mp3");
        var missing = LibraryRootPathResolver.ResolveExistingPath(root, "missing.mp3");

        Assert.True(existing.IsSuccess);
        Assert.Equal(file, existing.Value);
        Assert.False(missing.IsSuccess);
        Assert.Contains(missing.Errors, error =>
            string.Equals(error.Metadata["errorCode"]?.ToString(), "path.not_found", StringComparison.Ordinal));
    }

    private sealed class RootFixture : IDisposable
    {
        private RootFixture(string directory, Dictionary<string, string?> values, IConfiguration configuration)
        {
            Directory = directory;
            Values = values;
            Configuration = configuration;
        }

        public string Directory { get; }
        public Dictionary<string, string?> Values { get; }
        public IConfiguration Configuration { get; }

        public static RootFixture Create()
        {
            var directory = System.IO.Directory.CreateTempSubdirectory("dj-root-policy-").FullName;
            var values = new Dictionary<string, string?>
            {
                ["MAIN_LIBRARY_PATH"] = Path.Combine(directory, "main"),
                ["PENDING_LIBRARY_PATH"] = Path.Combine(directory, "pending"),
                ["REMEMBER_LIBRARY_PATH"] = Path.Combine(directory, "remember"),
                ["SESSIONS_LIBRARY_PATH"] = Path.Combine(directory, "sessions")
            };
            foreach (var path in values.Values)
            {
                System.IO.Directory.CreateDirectory(path!);
            }

            return new RootFixture(directory, values, new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        }

        public void Dispose() => System.IO.Directory.Delete(Directory, true);
    }
}
