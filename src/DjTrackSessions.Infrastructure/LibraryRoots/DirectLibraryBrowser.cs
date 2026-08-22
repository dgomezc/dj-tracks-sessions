using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Domain.LibraryScanning;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public sealed class DirectLibraryBrowser
{
    private readonly TagLibSharpAudioMetadataReader reader;

    public DirectLibraryBrowser(TagLibSharpAudioMetadataReader? reader = null) =>
        this.reader = reader ?? new TagLibSharpAudioMetadataReader();

    public FilesystemBrowseResponse Browse(LibraryRootPolicy root, string relativePath)
    {
        var resolved = LibraryRootPathResolver.ResolveExistingPath(root, relativePath);
        if (resolved.IsFailed)
        {
            throw new FilesystemBrowseException(ErrorCode(resolved.Errors), ErrorMessage(resolved.Errors));
        }

        if (!Directory.Exists(resolved.Value))
        {
            throw new FilesystemBrowseException("path.not_directory", "The requested path is not a directory.");
        }

        var directories = new List<FilesystemDirectoryItem>();
        var files = new List<FilesystemAudioItem>();
        var issues = new List<FilesystemBrowseIssue>();
        foreach (var entry in new DirectoryInfo(resolved.Value).EnumerateFileSystemInfos())
        {
            var entryPath = LibraryRootPathResolver.ResolveExistingPath(root, entry.FullName);
            if (entryPath.IsFailed)
            {
                issues.Add(new(Normalize(Path.GetRelativePath(root.CanonicalPath, entry.FullName)), "path.outside_root", "The entry escapes its configured root."));
                continue;
            }

            var entryRelativePath = Normalize(Path.GetRelativePath(root.CanonicalPath, entryPath.Value));
            if (entry is DirectoryInfo)
            {
                directories.Add(new(entry.Name, entryRelativePath));
                continue;
            }

            if (entry is FileInfo file && SupportedAudioFileTypes.IsSupported(file.Extension))
            {
                files.Add(ReadFile(root, file, entryRelativePath));
            }
        }

        return new(
            ToContract(root.Type),
            Normalize(Path.GetRelativePath(root.CanonicalPath, resolved.Value)),
            directories.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            files.OrderBy(item => item.FileName, StringComparer.OrdinalIgnoreCase).ToArray(),
            issues);
    }

    private FilesystemAudioItem ReadFile(LibraryRootPolicy root, FileInfo file, string relativePath)
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

        var tags = outcome.CurrentTags;
        var properties = outcome.TechnicalProperties;
        return new(
            relativePath,
            file.Name,
            file.Extension,
            tags?.Title,
            tags?.Artists ?? [],
            tags?.Album,
            tags?.Genres ?? [],
            tags?.Year,
            tags?.PersonalGenre,
            tags?.InitialKey,
            tags?.BeatsPerMinute,
            properties?.Duration,
            properties?.BitrateKbps,
            properties?.SampleRateHz,
            outcome.ErrorCode,
            outcome.ErrorMessage);
    }

    private static LibraryRoot ToContract(LibraryRootType type) => type switch
    {
        LibraryRootType.Main => LibraryRoot.Main,
        LibraryRootType.Pending => LibraryRoot.Pending,
        LibraryRootType.Remember => LibraryRoot.Remember,
        _ => throw new InvalidOperationException("Sessions is not a Track Management root.")
    };

    private static string Normalize(string path) => path == "." ? string.Empty : path.Replace(Path.DirectorySeparatorChar, '/');
    private static string ErrorCode(IEnumerable<FluentResults.IError> errors) => errors.FirstOrDefault()?.Metadata.TryGetValue("errorCode", out var value) == true ? value?.ToString() ?? "path.invalid" : "path.invalid";
    private static string ErrorMessage(IEnumerable<FluentResults.IError> errors) => errors.FirstOrDefault()?.Message ?? "The requested path is invalid.";
}

public sealed class FilesystemBrowseException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
