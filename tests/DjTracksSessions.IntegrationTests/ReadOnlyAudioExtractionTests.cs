using System.Security.Cryptography;
using System.Text;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Domain.LibraryScanning;
using DjTrackSessions.Infrastructure.LibraryRoots;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class ReadOnlyAudioExtractionTests
{
    [Fact]
    public void Extract_reads_supported_file_without_changing_source_and_continues_after_malformed_file()
    {
        using var fixture = FixtureRoot.Create();
        var validPath = fixture.Write("valid.wav", CreateWav());
        var malformedPath = fixture.Write("malformed.flac", [0x66, 0x4C, 0x61, 0x43, 0x00]);
        var validHash = Hash(validPath);
        var scan = new ReadOnlyLibraryScanner().Scan(fixture.Policy());

        var result = new ReadOnlyAudioExtractionCoordinator().Extract(fixture.Policy(), scan);

        Assert.Equal(2, result.Outcomes.Count);
        var valid = Assert.Single(result.Outcomes, outcome => outcome.File.FullPath == validPath);
        Assert.True(valid.IsSuccess);
        Assert.NotNull(valid.TechnicalProperties);
        Assert.NotNull(valid.CurrentTags);
        var malformed = Assert.Single(result.Outcomes, outcome => outcome.File.FullPath == malformedPath);
        Assert.False(malformed.IsSuccess);
        Assert.Equal("tag.read_failed", malformed.ErrorCode);
        Assert.True(malformed.IsHashSuccess);
        Assert.Equal(validHash, Hash(validPath));
    }

    [Fact]
    public void Extract_calculates_same_identity_for_identical_content_and_different_identity_for_changed_content()
    {
        using var fixture = FixtureRoot.Create();
        var first = fixture.Write("first.wav", CreateWav());
        var second = fixture.Write("second.wav", CreateWav());
        var changed = fixture.Write("changed.wav", [.. CreateWav(), 0x01]);
        var scan = new ReadOnlyLibraryScanner().Scan(fixture.Policy());

        var outcomes = new ReadOnlyAudioExtractionCoordinator().Extract(fixture.Policy(), scan).Outcomes;

        var firstHash = Assert.Single(outcomes, outcome => outcome.File.FullPath == first).ContentHash;
        var secondHash = Assert.Single(outcomes, outcome => outcome.File.FullPath == second).ContentHash;
        var changedHash = Assert.Single(outcomes, outcome => outcome.File.FullPath == changed).ContentHash;
        Assert.Equal(firstHash, secondHash);
        Assert.NotEqual(firstHash, changedHash);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(first))), firstHash);
    }

    [Fact]
    public void Extract_hashes_large_fixture_incrementally_and_preserves_source_bytes()
    {
        using var fixture = FixtureRoot.Create();
        var bytes = new byte[8 * 1024 * 1024];
        Random.Shared.NextBytes(bytes);
        var path = fixture.Write("large.wav", bytes);
        var before = Hash(path);
        var scan = new ReadOnlyLibraryScanner().Scan(fixture.Policy());

        var outcome = Assert.Single(new ReadOnlyAudioExtractionCoordinator().Extract(fixture.Policy(), scan).Outcomes);

        Assert.True(outcome.IsHashSuccess);
        Assert.Equal(before, Hash(path));
        Assert.Equal(before, outcome.ContentHash);
    }

    [Fact]
    public void Extract_reports_hash_failure_per_file_when_source_is_missing()
    {
        using var fixture = FixtureRoot.Create();
        var missing = new AudioFileDiscovery(Path.Combine(fixture.RootPath, "missing.wav"), "missing.wav", ".wav");
        var scan = new LibraryScanResult([missing], [], []);

        var outcome = Assert.Single(new ReadOnlyAudioExtractionCoordinator().Extract(fixture.Policy(), scan).Outcomes);

        Assert.False(outcome.IsHashSuccess);
        Assert.Equal("path.not_found", outcome.HashErrorCode);
        Assert.NotNull(outcome.HashErrorMessage);
    }

    [Fact]
    public void Reader_reports_unsupported_extension_as_a_per_file_failure()
    {
        using var fixture = FixtureRoot.Create();
        var path = fixture.Write("unsupported.ogg", Encoding.ASCII.GetBytes("not inspected"));
        var file = new AudioFileDiscovery(path, "unsupported.ogg", ".ogg");

        var result = new TagLibSharpAudioMetadataReader().Read(fixture.Policy(), file);

        Assert.False(result.IsSuccess);
        Assert.Equal("tag.unsupported", result.ErrorCode);
        Assert.Equal("not inspected", File.ReadAllText(path));
    }

    private static byte[] CreateWav() =>
        [.. Encoding.ASCII.GetBytes("RIFF"), 36, 0, 0, 0, .. Encoding.ASCII.GetBytes("WAVEfmt "),
         16, 0, 0, 0, 1, 0, 2, 0, 0x44, 0xac, 0, 0, 0, 0x10, 0xb1, 2, 0,
         4, 0, 16, 0, .. Encoding.ASCII.GetBytes("data"), 0, 0, 0, 0];

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private sealed class FixtureRoot : IDisposable
    {
        private FixtureRoot(string root) => Root = root;
        public string RootPath => Root;
        private string Root { get; }

        public static FixtureRoot Create() => new(Directory.CreateTempSubdirectory("dj-audio-extraction-").FullName);

        public string Write(string name, byte[] bytes)
        {
            var path = Path.Combine(Root, name);
            File.WriteAllBytes(path, bytes);
            return path;
        }

        public LibraryRootPolicy Policy() =>
            new(LibraryRootType.Main, Path.GetFullPath(Root), LibraryRootCapability.IndexAutomatically);

        public void Dispose() => Directory.Delete(Root, true);
    }
}
