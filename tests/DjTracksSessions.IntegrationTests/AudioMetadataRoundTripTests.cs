using System.Security.Cryptography;
using System.Text;
using System.Buffers.Binary;
using TagLibSharp2.Aiff;
using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Mp4;
using TagLibSharp2.Mpeg;
using TagLibSharp2.Riff;
using TagLibSharp2.Xiph;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class AudioMetadataRoundTripTests
{
    [Fact]
    public void Representative_formats_round_trip_in_disposable_fixture_root()
    {
        using var fixture = FixtureRoot.Create();
        var cases = new[]
        {
            RoundTripMp3(fixture),
            RoundTripFlac(fixture),
            RoundTripM4a(fixture),
            RoundTripAiff(fixture),
            RoundTripWav(fixture)
        };

        Assert.Equal(5, cases.Length);
        Assert.All(cases, result =>
        {
            Assert.NotEqual(result.SourceHash, result.OutputHash);
            Assert.Equal(result.SourceHash, Sha256(File.ReadAllBytes(result.SourcePath)));
            Assert.True(result.OutputLength > 0);
            Assert.NotEmpty(result.PreTechnicalEvidence);
            Assert.NotEmpty(result.PreTagEvidence);
            Assert.NotEmpty(result.TechnicalEvidence);
            Assert.Contains("Round Trip", result.TagEvidence);
        });
    }

    private static RoundTripEvidence RoundTripMp3(FixtureRoot root)
    {
        var source = root.Write("sample.mp3", CreateMp3());
        var original = File.ReadAllBytes(source);
        var parsed = AssertSuccess(Mp3File.Read(original)).File!;
        var preTechnical = parsed.Properties?.ToString() ?? "MPEG properties unavailable";
        var preTags = parsed.Title ?? "No initial title";
        parsed.Title = "Round Trip MP3";
        parsed.Artist = "Fixture Artist";
        parsed.Id3v2Tag!.SetUserText("UNKNOWN_MARKER", "preserve-me");
        var output = root.Output("sample.mp3", parsed.Render(original).ToArray());
        var reloaded = AssertSuccess(Mp3File.Read(File.ReadAllBytes(output))).File!;
        Assert.Equal("preserve-me", reloaded.Id3v2Tag!.GetUserText("UNKNOWN_MARKER"));
        return Evidence(source, output, preTechnical, preTags, reloaded.Properties?.ToString() ?? "MPEG properties unavailable", reloaded.Title!);
    }

    private static RoundTripEvidence RoundTripFlac(FixtureRoot root)
    {
        var source = root.Write("sample.flac", CreateFlac());
        var original = File.ReadAllBytes(source);
        var parsed = AssertSuccess(FlacFile.Read(original)).File!;
        var preTechnical = parsed.Properties.ToString();
        var preTags = parsed.VorbisComment!.Title ?? "No initial title";
        var tag = parsed.VorbisComment!;
        tag.Title = "Round Trip FLAC";
        tag.Artist = "Fixture Artist";
        tag.SetValue("UNKNOWN_MARKER", "preserve-me");
        var output = root.Output("sample.flac", parsed.Render(original).ToArray());
        var reloaded = AssertSuccess(FlacFile.Read(File.ReadAllBytes(output))).File!;
        Assert.Equal("preserve-me", reloaded.VorbisComment!.GetValue("UNKNOWN_MARKER"));
        return Evidence(source, output, preTechnical, preTags, reloaded.Properties.ToString(), reloaded.VorbisComment.Title!);
    }

    private static RoundTripEvidence RoundTripM4a(FixtureRoot root)
    {
        var source = root.Write("sample.m4a", CreateM4a());
        var original = File.ReadAllBytes(source);
        var parsed = AssertSuccess(Mp4File.Read(original)).File!;
        var preTechnical = parsed.Properties.ToString();
        var preTags = parsed.Title ?? "No initial title";
        parsed.Title = "Round Trip M4A";
        parsed.Artist = "Fixture Artist";
        var output = root.Output("sample.m4a", parsed.Render(original).ToArray());
        var reloaded = AssertSuccess(Mp4File.Read(File.ReadAllBytes(output))).File!;
        return Evidence(source, output, preTechnical, preTags, reloaded.Properties.ToString(), reloaded.Title!);
    }

    private static RoundTripEvidence RoundTripAiff(FixtureRoot root)
    {
        var source = root.Write("sample.aiff", CreateAiff());
        var original = File.ReadAllBytes(source);
        var parsed = AssertSuccess(AiffFile.Read(original)).File!;
        var preTechnical = parsed.Properties?.ToString() ?? "AIFF properties unavailable";
        var preTags = parsed.Tag?.Title ?? "No tags";
        parsed.Tag = new Id3v2Tag { Title = "Round Trip AIFF", Artist = "Fixture Artist" };
        var output = root.Output("sample.aiff", parsed.Render().ToArray());
        var reloaded = AssertSuccess(AiffFile.Read(File.ReadAllBytes(output))).File!;
        return Evidence(source, output, preTechnical, preTags, reloaded.Properties?.ToString() ?? "AIFF properties unavailable", reloaded.Tag!.Title!);
    }

    private static RoundTripEvidence RoundTripWav(FixtureRoot root)
    {
        var source = root.Write("sample.wav", CreateWav());
        var original = File.ReadAllBytes(source);
        var parsed = AssertSuccess(WavFile.Read(original)).File!;
        var preTechnical = parsed.Properties.ToString();
        var preTags = parsed.InfoTag?.Title ?? "No tags";
        parsed.InfoTag = new RiffInfoTag { Title = "Round Trip WAV", Artist = "Fixture Artist" };
        var output = root.Output("sample.wav", parsed.Render().ToArray());
        var reloaded = AssertSuccess(WavFile.Read(File.ReadAllBytes(output))).File!;
        return Evidence(source, output, preTechnical, preTags, reloaded.Properties.ToString(), reloaded.InfoTag!.Title!);
    }

    private static RoundTripEvidence Evidence(string source, string output, string preTechnical, string preTags, string technical, string tags) =>
        new(source, output, Sha256(File.ReadAllBytes(source)), Sha256(File.ReadAllBytes(output)), new FileInfo(output).Length, preTechnical, preTags, technical, tags);

    private static T AssertSuccess<T>(T result) => result;

    private static byte[] CreateMp3() => [.. new Id3v2Tag { Title = "Original MP3", Artist = "Original Artist" }.Render(), .. Enumerable.Repeat((byte)0, 256)];

    private static byte[] CreateFlac()
    {
        var data = new List<byte>(Encoding.ASCII.GetBytes("fLaC"));
        data.AddRange([0x00, 0x00, 0x00, 0x22]);
        data.AddRange([0x10, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0xC4, 0x42, 0xF0, 0x00, 0x00, 0x00, 0x00]);
        data.AddRange(new byte[18]);
        var comment = new VorbisComment("fixture") { Title = "Original FLAC", Artist = "Original Artist" }.Render();
        data.Add(0x84);
        data.Add((byte)(comment.Length >> 16)); data.Add((byte)(comment.Length >> 8)); data.Add((byte)comment.Length);
        data.AddRange(comment.ToArray());
        return [.. data];
    }

    private static byte[] CreateM4a()
    {
        static byte[] Box(string type, byte[] data)
        {
            var result = new byte[8 + data.Length];
            BinaryPrimitives.WriteUInt32BigEndian(result, (uint)result.Length);
            Encoding.ASCII.GetBytes(type).CopyTo(result, 4);
            data.CopyTo(result, 8);
            return result;
        }

        static byte[] FullBox(string type, byte[] data) => Box(type, [0, 0, 0, 0, .. data]);

        var titleData = FullBox("data", [0, 0, 0, 1, 0, 0, 0, 0, .. Encoding.UTF8.GetBytes("Original M4A")]);
        var artistData = FullBox("data", [0, 0, 0, 1, 0, 0, 0, 0, .. Encoding.UTF8.GetBytes("Original Artist")]);
        var ilst = Box("ilst", [.. Box("©nam", titleData), .. Box("©ART", artistData)]);
        var hdlr = FullBox("hdlr", [0, 0, 0, 0, .. Encoding.ASCII.GetBytes("mdirappl"), 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        var meta = FullBox("meta", [.. hdlr, .. ilst]);
        var moov = Box("moov", Box("udta", meta));
        return [.. Box("ftyp", [.. Encoding.ASCII.GetBytes("M4A "), 0, 0, 0, 0, .. Encoding.ASCII.GetBytes("M4A ")]), .. moov, .. Box("mdat", [0, 0, 0, 0])];
    }

    private static byte[] CreateAiff() => [.. Encoding.ASCII.GetBytes("FORM"), 0, 0, 0, 38, .. Encoding.ASCII.GetBytes("AIFFCOMM"), 0, 0, 0, 18, 0, 2, 0, 0, 0, 0, 0, 0, 0, 16, 0x40, 0x0e, 0xac, 0x44, 0, 0, 0, 0, 0, 0, .. Encoding.ASCII.GetBytes("SSND"), 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0];

    private static byte[] CreateWav() => [.. Encoding.ASCII.GetBytes("RIFF"), 36, 0, 0, 0, .. Encoding.ASCII.GetBytes("WAVEfmt "), 16, 0, 0, 0, 1, 0, 2, 0, 0x44, 0xac, 0, 0, 0, 0, 0, 0, 4, 0, 16, 0, .. Encoding.ASCII.GetBytes("data"), 0, 0, 0, 0];

    private static string Sha256(byte[] data) => Convert.ToHexString(SHA256.HashData(data));

    private sealed record RoundTripEvidence(string SourcePath, string OutputPath, string SourceHash, string OutputHash, long OutputLength, string PreTechnicalEvidence, string PreTagEvidence, string TechnicalEvidence, string TagEvidence);

    private sealed class FixtureRoot : IDisposable
    {
        private FixtureRoot(string root) => Root = root;
        private string Root { get; }
        public static FixtureRoot Create() => new(Directory.CreateTempSubdirectory("dj-audio-roundtrip-").FullName);
        public string Write(string name, byte[] data) { var path = Path.Combine(Root, name); File.WriteAllBytes(path, data); return path; }
        public string Output(string name, byte[] data) => Write($"out-{name}", data);
        public void Dispose() => Directory.Delete(Root, true);
    }
}
