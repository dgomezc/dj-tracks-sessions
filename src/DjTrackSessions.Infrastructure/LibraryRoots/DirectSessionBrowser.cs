using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Domain.LibraryScanning;
using TagLibSharp2.Mpeg;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public sealed class DirectSessionBrowser
{
    private readonly TagLibSharpAudioMetadataReader reader;
    private readonly SessionTracklistResolver tracklists;

    public DirectSessionBrowser(TagLibSharpAudioMetadataReader? reader = null, SessionTracklistResolver? tracklists = null)
    {
        this.reader = reader ?? new TagLibSharpAudioMetadataReader();
        this.tracklists = tracklists ?? new SessionTracklistResolver();
    }

    public SessionBrowseResponse Browse(LibraryRootPolicy root, string relativePath)
    {
        var resolved = LibraryRootPathResolver.ResolveExistingPath(root, relativePath);
        if (resolved.IsFailed)
        {
            throw new SessionBrowseException(ErrorCode(resolved.Errors), ErrorMessage(resolved.Errors));
        }

        if (!Directory.Exists(resolved.Value))
        {
            throw new SessionBrowseException("path.not_directory", "The requested path is not a directory.");
        }

        var directories = new List<SessionDirectoryItem>();
        var files = new List<SessionAudioItem>();
        var issues = new List<SessionBrowseIssue>();
        foreach (var entry in new DirectoryInfo(resolved.Value).EnumerateFileSystemInfos())
        {
            var entryPath = LibraryRootPathResolver.ResolveExistingPath(root, entry.FullName);
            var entryRelativePath = Normalize(Path.GetRelativePath(root.CanonicalPath, entry.FullName));
            if (entryPath.IsFailed)
            {
                issues.Add(new(entryRelativePath, "path.outside_root", "The entry escapes its configured root."));
                continue;
            }

            if (entry is DirectoryInfo)
            {
                directories.Add(new(entry.Name, entryRelativePath));
                continue;
            }

            if (entry is FileInfo file && SupportedAudioFileTypes.IsSessionAudio(file.Extension))
            {
                files.Add(ReadFile(root, file, entryRelativePath));
            }
        }

        return new(
            Normalize(Path.GetRelativePath(root.CanonicalPath, resolved.Value)),
            directories.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            files.OrderBy(item => item.FileName, StringComparer.OrdinalIgnoreCase).ToArray(),
            issues);
    }

    public SessionDetail Detail(LibraryRootPolicy root, string relativePath)
    {
        var resolved = LibraryRootPathResolver.ResolveExistingPath(root, relativePath);
        if (resolved.IsFailed) throw new SessionBrowseException(ErrorCode(resolved.Errors), ErrorMessage(resolved.Errors));
        if (!File.Exists(resolved.Value)) throw new SessionBrowseException("path.not_file", "The requested path is not an audio file.");

        var file = new FileInfo(resolved.Value);
        if (!SupportedAudioFileTypes.IsSessionAudio(file.Extension))
        {
            return new(new(relativePath), file.Name, file.Extension, null, [], null, [], null, null, null, null, null, null, "tag.unsupported", "The audio extension is not supported for Sessions metadata.");
        }
        var outcome = reader.Read(root, new(relativePath, relativePath, file.Extension));
        if (outcome.ErrorCode == "tag.properties_unavailable" && string.Equals(file.Extension, ".mp3", StringComparison.OrdinalIgnoreCase))
        {
            var parsed = Mp3File.Read(File.ReadAllBytes(file.FullName));
            if (parsed.IsSuccess && parsed.File is not null)
            {
                var tag = parsed.File;
                return new(new(relativePath), file.Name, file.Extension, tag.Title, Split(tag.Artist), tag.Album, Split(tag.Genre), uint.TryParse(tag.Year, out var year) ? year : null, null, null, null, null, null, null, null, tracklists.Resolve(root, relativePath));
            }
        }
        return ToDetail(root, relativePath, file, outcome);
    }

    private SessionAudioItem ReadFile(LibraryRootPolicy root, FileInfo file, string relativePath)
    {
        var discovery = new AudioFileDiscovery(file.FullName, relativePath, file.Extension);
        AudioExtractionOutcome outcome;
        try
        {
            outcome = reader.Read(root, discovery);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or FormatException)
        {
            outcome = AudioExtractionOutcome.Failure(discovery, "tag.read_failed", exception.Message);
        }

        return new(
            relativePath,
            file.Name,
            file.Extension,
            outcome.CurrentTags?.Title,
            outcome.CurrentTags?.Year,
            outcome.TechnicalProperties?.Duration,
            outcome.ErrorCode,
            outcome.ErrorMessage);
    }

    private SessionDetail ToDetail(LibraryRootPolicy root, string relativePath, FileInfo file, AudioExtractionOutcome outcome) =>
        new(new(relativePath), file.Name, file.Extension,
            outcome.CurrentTags?.Title, outcome.CurrentTags?.Artists ?? [], outcome.CurrentTags?.Album,
            outcome.CurrentTags?.Genres ?? [], outcome.CurrentTags?.Year, outcome.CurrentTags?.InitialKey,
            outcome.CurrentTags?.BeatsPerMinute, outcome.TechnicalProperties?.Duration,
            outcome.TechnicalProperties?.BitrateKbps, outcome.TechnicalProperties?.SampleRateHz,
            outcome.ErrorCode, outcome.ErrorMessage, tracklists.Resolve(root, relativePath));

    private static IReadOnlyList<string> Split(string? value) => value?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];

    private static string Normalize(string path) => path == "." ? string.Empty : path.Replace(Path.DirectorySeparatorChar, '/');
    private static string ErrorCode(IEnumerable<FluentResults.IError> errors) => errors.FirstOrDefault()?.Metadata.TryGetValue("errorCode", out var value) == true ? value?.ToString() ?? "path.invalid" : "path.invalid";
    private static string ErrorMessage(IEnumerable<FluentResults.IError> errors) => errors.FirstOrDefault()?.Message ?? "The requested path is invalid.";
}

public sealed class SessionBrowseException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
