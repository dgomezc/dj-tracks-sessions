using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Domain.LibraryScanning;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public sealed class ReadOnlyLibraryScanner
{
    public LibraryScanResult Scan(LibraryRootPolicy root)
    {
        return root.Type == LibraryRootType.Sessions
            ? ScanSessions(root)
            : ScanOrdinaryRoot(root);
    }

    private static LibraryScanResult ScanOrdinaryRoot(LibraryRootPolicy root)
    {
        var files = new List<AudioFileDiscovery>();
        var issues = new List<string>();
        Walk(root, root.CanonicalPath, files, issues, new HashSet<string>(StringComparer.Ordinal));
        return new(files, [], issues);
    }

    private static LibraryScanResult ScanSessions(LibraryRootPolicy root)
    {
        var sessions = new List<SessionFolderDiscovery>();
        var issues = new List<string>();
        var rootInfo = new DirectoryInfo(root.CanonicalPath);

        foreach (var entry in rootInfo.EnumerateFileSystemInfos())
        {
            var resolved = LibraryRootPathResolver.ResolveExistingPath(root, entry.FullName);
            if (resolved.IsFailed)
            {
                issues.Add(entry.FullName);
                continue;
            }

            if (entry is not DirectoryInfo)
            {
                continue;
            }

            sessions.Add(InspectSessionFolder(root, entry.Name, resolved.Value));
        }

        return new([], sessions, issues);
    }

    private static SessionFolderDiscovery InspectSessionFolder(
        LibraryRootPolicy root,
        string relativePath,
        string fullPath)
    {
        var audio = new List<AudioFileDiscovery>();
        var tracklists = new List<string>();
        var artwork = new List<string>();
        var issues = new List<string>();

        foreach (var entry in new DirectoryInfo(fullPath).EnumerateFileSystemInfos())
        {
            var resolved = LibraryRootPathResolver.ResolveExistingPath(root, entry.FullName);
            if (resolved.IsFailed)
            {
                issues.Add(entry.Name);
                continue;
            }

            if (entry is not FileInfo file)
            {
                continue;
            }

            var extension = file.Extension;
            if (SupportedAudioFileTypes.IsSessionAudio(extension))
            {
                audio.Add(new(file.FullName, Path.Combine(relativePath, file.Name), extension));
            }
            else if (string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase))
            {
                tracklists.Add(Path.Combine(relativePath, file.Name));
            }
            else if (SupportedAudioFileTypes.IsSessionArtwork(extension))
            {
                artwork.Add(Path.Combine(relativePath, file.Name));
            }
        }

        if (audio.Count == 0)
        {
            issues.Add("session.audio_missing");
        }

        if (tracklists.Count == 0)
        {
            issues.Add("session.tracklist_missing");
        }

        return new(fullPath, relativePath, audio, tracklists, artwork, issues);
    }

    private static void Walk(
        LibraryRootPolicy root,
        string directory,
        List<AudioFileDiscovery> files,
        List<string> issues,
        HashSet<string> visitedDirectories)
    {
        if (!visitedDirectories.Add(directory))
        {
            return;
        }

        foreach (var entry in new DirectoryInfo(directory).EnumerateFileSystemInfos())
        {
            var resolved = LibraryRootPathResolver.ResolveExistingPath(root, entry.FullName);
            if (resolved.IsFailed)
            {
                issues.Add(entry.FullName);
                continue;
            }

            if (entry is DirectoryInfo)
            {
                Walk(root, resolved.Value, files, issues, visitedDirectories);
                continue;
            }

            if (entry is FileInfo file && SupportedAudioFileTypes.IsSupported(file.Extension))
            {
                files.Add(new(
                    file.FullName,
                    Path.GetRelativePath(root.CanonicalPath, file.FullName),
                    file.Extension));
            }
        }
    }
}
