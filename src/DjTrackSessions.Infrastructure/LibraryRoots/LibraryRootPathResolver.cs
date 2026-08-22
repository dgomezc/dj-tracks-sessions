using DjTrackSessions.Domain.LibraryRoots;
using FluentResults;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public static class LibraryRootPathResolver
{
    public static Result<string> ResolveExistingPath(LibraryRootPolicy root, string requestedPath)
    {
        if (requestedPath is null)
        {
            return OutsideRoot();
        }

        string fullPath;
        try
        {
            if (Path.IsPathFullyQualified(requestedPath))
            {
                fullPath = Path.GetFullPath(requestedPath);
                if (!IsWithinRootLexically(fullPath, root.CanonicalPath))
                {
                    return OutsideRoot();
                }
            }
            else
            {
                var relativePath = Path.GetFullPath(requestedPath, root.CanonicalPath);
                if (!IsWithinRootLexically(relativePath, root.CanonicalPath))
                {
                    return OutsideRoot();
                }

                fullPath = relativePath;
            }
        }
        catch (ArgumentException)
        {
            return OutsideRoot();
        }

        if (!TryResolveExisting(fullPath, out var resolvedPath, out var exists))
        {
            return exists ? OutsideRoot() : NotFound();
        }

        return IsWithinRoot(resolvedPath, root.CanonicalPath) ? Result.Ok(resolvedPath) : OutsideRoot();
    }

    public static Result<string> ResolvePathUnderRoot(LibraryRootPolicy root, string requestedPath)
    {
        if (requestedPath is null || Path.IsPathFullyQualified(requestedPath)) return OutsideRoot();

        try
        {
            var lexicalPath = Path.GetFullPath(requestedPath, root.CanonicalPath);
            if (!IsWithinRootLexically(lexicalPath, root.CanonicalPath)) return OutsideRoot();

            var relative = Path.GetRelativePath(root.CanonicalPath, lexicalPath);
            var resolvedPath = root.CanonicalPath;
            foreach (var segment in relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                var candidate = Path.Combine(resolvedPath, segment);
                if (!Directory.Exists(candidate) && !File.Exists(candidate))
                {
                    resolvedPath = Path.Combine(resolvedPath, segment);
                    continue;
                }

                var entry = (FileSystemInfo)new DirectoryInfo(candidate);
                if (entry is DirectoryInfo directory && !directory.Exists) entry = new FileInfo(candidate);
                resolvedPath = entry.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? candidate;
                if (!IsWithinRoot(resolvedPath, root.CanonicalPath)) return OutsideRoot();
            }

            return IsWithinRoot(resolvedPath, root.CanonicalPath)
                ? Result.Ok(Path.GetFullPath(resolvedPath))
                : OutsideRoot();
        }
        catch (ArgumentException)
        {
            return OutsideRoot();
        }
    }

    internal static bool TryCanonicalizeDirectory(string configuredPath, out string canonicalPath)
    {
        canonicalPath = string.Empty;
        try
        {
            var fullPath = Path.GetFullPath(configuredPath.Trim());
            if (!TryResolveExisting(fullPath, out var resolvedPath, out var exists) || !exists || !Directory.Exists(resolvedPath))
            {
                return false;
            }

            canonicalPath = resolvedPath;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    internal static bool PathsOverlap(string left, string right) =>
        IsWithinRoot(left, right) || IsWithinRoot(right, left);

    private static bool TryResolveExisting(string fullPath, out string resolvedPath, out bool exists)
    {
        resolvedPath = Path.GetPathRoot(fullPath)!;
        exists = true;

        var relative = Path.GetRelativePath(resolvedPath, fullPath);
        if (relative == ".")
        {
            return true;
        }

        foreach (var segment in relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            var candidate = Path.Combine(resolvedPath, segment);
            var entry = (FileSystemInfo)new DirectoryInfo(candidate);
            if (!entry.Exists && !File.Exists(candidate))
            {
                exists = false;
                return false;
            }

            if (entry is DirectoryInfo directory && !directory.Exists)
            {
                entry = new FileInfo(candidate);
            }

            var linkTarget = entry.ResolveLinkTarget(returnFinalTarget: true);
            resolvedPath = linkTarget?.FullName ?? candidate;
        }

        resolvedPath = Path.GetFullPath(resolvedPath);
        return true;
    }

    private static bool IsWithinRoot(string path, string root)
    {
        var relative = Path.GetRelativePath(root, path);
        return relative == "." || IsWithinRootLexically(path, root);
    }

    private static bool IsWithinRootLexically(string path, string root)
    {
        var relative = Path.GetRelativePath(root, path);
        return relative != ".." &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relative);
    }

    private static Result<string> NotFound() =>
        Result.Fail<string>(new Error("The requested path does not exist.")
            .WithMetadata("errorCode", "path.not_found"));

    private static Result<string> OutsideRoot() =>
        Result.Fail<string>(new Error("The requested path escapes its configured root.")
            .WithMetadata("errorCode", "path.outside_root"));
}
