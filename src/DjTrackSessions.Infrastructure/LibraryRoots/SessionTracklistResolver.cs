using System.Text;
using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Domain.LibraryScanning;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public sealed class SessionTracklistResolver
{
    public SessionTracklist Resolve(LibraryRootPolicy root, string audioRelativePath)
    {
        var audioResult = LibraryRootPathResolver.ResolveExistingPath(root, audioRelativePath);
        if (audioResult.IsFailed) return new("missing", null, null, []);

        var directory = Path.GetDirectoryName(audioResult.Value);
        if (directory is null) return new("missing", null, null, []);

        var specific = ReadCandidate(root, Path.Combine(directory, Path.GetFileNameWithoutExtension(audioResult.Value) + ".txt"));
        if (specific is not null) return specific;

        var genericPath = Path.Combine(directory, "tracklist.txt");
        if (!ExistsUnderRoot(root, genericPath)) return new("missing", null, null, []);

        var audioFiles = Directory.EnumerateFiles(directory)
            .Where(path => SupportedAudioFileTypes.IsSessionAudio(Path.GetExtension(path)))
            .Select(path => LibraryRootPathResolver.ResolveExistingPath(root, path))
            .Where(result => result.IsSuccess)
            .Select(result => Normalize(Path.GetRelativePath(root.CanonicalPath, result.Value)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (audioFiles.Length != 1)
            return new("ambiguous", Normalize(Path.GetRelativePath(root.CanonicalPath, genericPath)), null, audioFiles);

        return ReadCandidate(root, genericPath) ?? new("missing", null, null, audioFiles);
    }

    private static SessionTracklist? ReadCandidate(LibraryRootPolicy root, string path)
    {
        var resolved = LibraryRootPathResolver.ResolveExistingPath(root, path);
        if (resolved.IsFailed || !File.Exists(resolved.Value)) return null;
        var relativePath = Normalize(Path.GetRelativePath(root.CanonicalPath, resolved.Value));
        try
        {
            var text = new UTF8Encoding(false, true).GetString(File.ReadAllBytes(resolved.Value));
            return new("resolved", relativePath, text, []);
        }
        catch (DecoderFallbackException) { return new("invalid", relativePath, null, []); }
        catch (IOException) { return new("unreadable", relativePath, null, []); }
        catch (UnauthorizedAccessException) { return new("unreadable", relativePath, null, []); }
    }

    private static bool ExistsUnderRoot(LibraryRootPolicy root, string path) =>
        LibraryRootPathResolver.ResolveExistingPath(root, path).IsSuccess;

    private static string Normalize(string path) => path == "." ? string.Empty : path.Replace(Path.DirectorySeparatorChar, '/');
}
