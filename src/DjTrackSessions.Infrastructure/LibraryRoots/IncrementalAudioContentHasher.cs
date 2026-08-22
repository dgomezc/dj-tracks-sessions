using System.Security.Cryptography;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Domain.LibraryScanning;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public sealed class IncrementalAudioContentHasher
{
    private const int BufferSize = 64 * 1024;

    public HashOutcome Hash(LibraryRootPolicy root, AudioFileDiscovery file)
    {
        var resolved = LibraryRootPathResolver.ResolveExistingPath(root, file.FullPath);
        if (resolved.IsFailed)
        {
            var error = resolved.Errors[0];
            return HashOutcome.Failure(
                error.Metadata.TryGetValue("errorCode", out var code) ? code?.ToString() ?? "hash.read_failed" : "hash.read_failed",
                error.Message);
        }

        try
        {
            using var stream = new FileStream(
                resolved.Value,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BufferSize,
                FileOptions.SequentialScan);
            using var algorithm = SHA256.Create();
            var buffer = new byte[BufferSize];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                algorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
            }

            algorithm.TransformFinalBlock([], 0, 0);
            return HashOutcome.Success(Convert.ToHexString(algorithm.Hash!));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return HashOutcome.Failure("hash.read_failed", exception.Message);
        }
    }

    public sealed record HashOutcome(string? ContentHash, string? ErrorCode, string? ErrorMessage)
    {
        public static HashOutcome Success(string contentHash) => new(contentHash, null, null);

        public static HashOutcome Failure(string errorCode, string errorMessage) =>
            new(null, errorCode, errorMessage);
    }
}
