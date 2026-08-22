using DjTrackSessions.Domain.LibraryScanning;
using DjTrackSessions.Domain.LibraryRoots;
using TagLibSharp2.Aiff;
using TagLibSharp2.Core;
using TagLibSharp2.Mp4;
using TagLibSharp2.Mpeg;
using TagLibSharp2.Riff;
using TagLibSharp2.Xiph;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public sealed class TagLibSharpAudioMetadataReader
{
    public AudioExtractionOutcome Read(LibraryRootPolicy root, AudioFileDiscovery file)
    {
        var resolved = LibraryRootPathResolver.ResolveExistingPath(root, file.FullPath);
        if (resolved.IsFailed)
        {
            return AudioExtractionOutcome.Failure(file, "path.outside_root", "The discovered file is no longer inside its configured root.");
        }

        try
        {
            var bytes = File.ReadAllBytes(resolved.Value);
            var extension = file.Extension.ToLowerInvariant();
            return extension switch
            {
                ".mp3" => ReadMp3(file, bytes),
                ".flac" => ReadFlac(file, bytes),
                ".m4a" or ".aac" => ReadMp4(file, bytes),
                ".aiff" => ReadAiff(file, bytes),
                ".wav" => ReadWav(file, bytes),
                _ => AudioExtractionOutcome.Failure(file, "tag.unsupported", "The audio extension is not supported for metadata extraction.")
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return AudioExtractionOutcome.Failure(file, "tag.read_failed", exception.Message);
        }
    }

    private static AudioExtractionOutcome ReadMp3(AudioFileDiscovery file, byte[] bytes)
    {
        var result = Mp3File.Read(bytes);
        return result.IsSuccess && result.File is not null
            ? Success(file, result.File, result.File.Tag)
            : Failure(file, result.Error);
    }

    private static AudioExtractionOutcome ReadFlac(AudioFileDiscovery file, byte[] bytes)
    {
        var result = FlacFile.Read(bytes);
        return result.IsSuccess && result.File is not null
            ? Success(file, result.File, result.File.Tag)
            : Failure(file, result.Error);
    }

    private static AudioExtractionOutcome ReadMp4(AudioFileDiscovery file, byte[] bytes)
    {
        var result = Mp4File.Read(bytes);
        return result.IsSuccess && result.File is not null
            ? Success(file, result.File, result.File.Tag)
            : Failure(file, result.Error);
    }

    private static AudioExtractionOutcome ReadAiff(AudioFileDiscovery file, byte[] bytes)
    {
        var result = AiffFile.Read(bytes);
        return result.IsSuccess && result.File is not null
            ? Success(file, result.File, result.File.Tag)
            : Failure(file, result.Error);
    }

    private static AudioExtractionOutcome ReadWav(AudioFileDiscovery file, byte[] bytes)
    {
        var result = WavFile.Read(bytes);
        return result.IsSuccess && result.File is not null
            ? Success(file, result.File, result.File.InfoTag)
            : Failure(file, result.Error);
    }

    private static AudioExtractionOutcome Success(
        AudioFileDiscovery file,
        object media,
        object? tag)
    {
        var properties = Get(media, "Properties");
        return properties is null
            ? AudioExtractionOutcome.Failure(file, "tag.properties_unavailable", "Audio properties could not be extracted.")
            : AudioExtractionOutcome.Success(file, new(
                GetValue<TimeSpan>(properties, "Duration"),
                GetValue<int>(properties, "Bitrate"),
                GetValue<int>(properties, "SampleRate"),
                GetValue<int>(properties, "BitsPerSample"),
                GetValue<int>(properties, "Channels"),
                GetValue<string>(properties, "Codec")), ToTags(tag));
    }

    private static AudioExtractionOutcome Failure(AudioFileDiscovery file, string? error) =>
        AudioExtractionOutcome.Failure(file, "tag.read_failed", error ?? "TagLibSharp2 could not read the file.");

    private static CurrentAudioTags ToTags(object? tag) => tag is null
        ? new(null, [], null, [], [], null, null, null, null, null, null, null, null, null, null)
        : new(
            GetValue<string>(tag, "Title"),
            Values(Get(tag, "Performers"), GetValue<string>(tag, "Artist")),
            GetValue<string>(tag, "Album"),
            Values(Get(tag, "AlbumArtists"), GetValue<string>(tag, "AlbumArtist")),
            Values(Get(tag, "Genres"), GetValue<string>(tag, "Genre")),
            ParseUInt(GetValue<string>(tag, "Year")),
            GetValue<uint?>(tag, "Track"),
            null,
            GetValue<uint?>(tag, "DiscNumber"),
            null,
            GetValue<string>(tag, "Comment"),
            GetValue<string>(tag, "Composer"),
            GetValue<string>(tag, "Isrc"),
            GetValue<string>(tag, "InitialKey"),
            GetValue<string>(tag, "BeatsPerMinute"));

    private static object? Get(object instance, string name) =>
        instance.GetType().GetProperty(name)?.GetValue(instance);

    private static T? GetValue<T>(object instance, string name)
    {
        var value = Get(instance, name);
        return value is T typed ? typed : default;
    }

    private static IReadOnlyList<string> Values(object? values, string? fallback)
    {
        var result = values is IEnumerable<string> many
            ? many.Where(value => !string.IsNullOrWhiteSpace(value)).ToList()
            : [];
        if (result.Count == 0 && !string.IsNullOrWhiteSpace(fallback))
        {
            result.Add(fallback);
        }

        return result;
    }

    private static uint? ParseUInt(string? value) =>
        uint.TryParse(value, out var parsed) ? parsed : null;
}
