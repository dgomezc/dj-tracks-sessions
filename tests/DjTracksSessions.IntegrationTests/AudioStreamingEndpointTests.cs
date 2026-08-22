using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class AudioStreamingEndpointTests : IClassFixture<AudioStreamingEndpointTests.ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public AudioStreamingEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Streams_full_file_with_format_content_type()
    {
        var bytes = Enumerable.Range(0, 32).Select(value => (byte)value).ToArray();
        _factory.Write("main", "track.mp3", bytes);

        var response = await _client.GetAsync("/playback/stream?root=Main&path=track.mp3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("audio/mpeg", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(bytes, await response.Content.ReadAsByteArrayAsync());
        Assert.Contains("bytes", response.Headers.AcceptRanges);
    }

    [Fact]
    public async Task Streams_valid_range_without_full_file()
    {
        var bytes = Enumerable.Range(0, 32).Select(value => (byte)value).ToArray();
        _factory.Write("pending", "track.wav", bytes);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/playback/stream?root=Pending&path=track.wav");
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(4, 9);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal("audio/wav", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(6, response.Content.Headers.ContentLength);
        Assert.Equal(bytes[4..10], await response.Content.ReadAsByteArrayAsync());
        Assert.Equal("bytes 4-9/32", response.Content.Headers.ContentRange?.ToString());
    }

    [Fact]
    public async Task Rejects_invalid_range()
    {
        _factory.Write("remember", "track.flac", [1, 2, 3, 4]);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/playback/stream?root=Remember&path=track.flac");
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(20, 30);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
    }

    [Theory]
    [InlineData("Main", "missing.mp3", "path.not_found")]
    [InlineData("Main", "../outside.mp3", "path.outside_root")]
    [InlineData("Sessions", "session.mp3", "playback.root_invalid")]
    [InlineData("Main", "notes.txt", "audio.unsupported_format")]
    [InlineData("Main", "/tmp/track.mp3", "path.outside_root")]
    public async Task Rejects_invalid_stream_requests(string root, string path, string errorCode)
    {
        var response = await _client.GetAsync($"/playback/stream?root={root}&path={Uri.EscapeDataString(path)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(errorCode, document.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Rejects_symlink_escape()
    {
        var outside = Path.Combine(_factory.Root, "outside");
        Directory.CreateDirectory(outside);
        File.WriteAllBytes(Path.Combine(outside, "track.mp3"), [1, 2, 3]);
        Directory.CreateSymbolicLink(Path.Combine(_factory.Root, "main", "escape"), outside);

        var response = await _client.GetAsync("/playback/stream?root=Main&path=escape/track.mp3");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("path.outside_root", document.RootElement.GetProperty("errorCode").GetString());
    }

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "dj-streaming-" + Guid.NewGuid().ToString("N"));

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            Directory.CreateDirectory(Root);
            foreach (var name in new[] { "main", "pending", "remember", "sessions" })
            {
                Directory.CreateDirectory(Path.Combine(Root, name));
            }

            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MAIN_LIBRARY_PATH"] = Path.Combine(Root, "main"),
                ["PENDING_LIBRARY_PATH"] = Path.Combine(Root, "pending"),
                ["REMEMBER_LIBRARY_PATH"] = Path.Combine(Root, "remember"),
                ["SESSIONS_LIBRARY_PATH"] = Path.Combine(Root, "sessions")
            }));
        }

        public void Write(string root, string path, byte[] bytes) =>
            File.WriteAllBytes(Path.Combine(Root, root, path), bytes);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
