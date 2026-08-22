using DjTracksSessions.Api;
using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Domain.LibraryScanning;
using DjTrackSessions.Infrastructure.LibraryRoots;
using FluentResults;

namespace DjTracksSessions.Api.Features.Playback;

public static class AudioStreamingEndpoint
{
    private static readonly IReadOnlyDictionary<string, string> ContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".mp3"] = "audio/mpeg",
            [".flac"] = "audio/flac",
            [".m4a"] = "audio/mp4",
            [".aac"] = "audio/aac",
            [".aiff"] = "audio/aiff",
            [".wav"] = "audio/wav"
        };

    public static IEndpointRouteBuilder MapAudioStreaming(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/playback/stream", (string? root, string? path, IConfiguration configuration) =>
        {
            if (!Enum.TryParse<LibraryRoot>(root, ignoreCase: true, out var requestedRoot))
            {
                return ApiProblemDetails.FromError(Failure("playback.root_invalid", "A Main, Pending, or Remember root is required."));
            }

            if (string.IsNullOrWhiteSpace(path) || Path.IsPathFullyQualified(path))
            {
                return ApiProblemDetails.FromError(Failure("path.outside_root", "Only a relative path is accepted."));
            }

            var policies = LibraryRootConfiguration.LoadTrackManagement(configuration);
            if (policies.IsFailed)
            {
                return ApiProblemDetails.FromResult(Result.Fail<string>(policies.Errors), _ => Results.Empty);
            }

            var policy = policies.Value.SingleOrDefault(item => ToContract(item.Type) == requestedRoot);
            if (policy is null || !policy.Capabilities.HasFlag(LibraryRootCapability.BrowseAndPlay))
            {
                return ApiProblemDetails.FromError(Failure("playback.root_invalid", "The requested root is not available for playback."));
            }

            var extension = Path.GetExtension(path);
            if (!SupportedAudioFileTypes.IsSupported(extension) || !ContentTypes.ContainsKey(extension))
            {
                return ApiProblemDetails.FromError(Failure("audio.unsupported_format", "The requested file format is not supported for playback."));
            }

            var resolved = LibraryRootPathResolver.ResolveExistingPath(policy, path);
            if (resolved.IsFailed)
            {
                return ApiProblemDetails.FromError(resolved.Errors.First());
            }

            if (!File.Exists(resolved.Value))
            {
                return ApiProblemDetails.FromError(Failure("path.not_file", "The requested path is not a file."));
            }

            try
            {
                var stream = new FileStream(resolved.Value, FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: 64 * 1024, options: FileOptions.Asynchronous | FileOptions.SequentialScan);
                return Results.Stream(stream, ContentTypes[extension], enableRangeProcessing: true);
            }
            catch (FileNotFoundException)
            {
                return ApiProblemDetails.FromError(ApiProblemDetails.NotFound("The requested file does not exist.", "path.not_found"));
            }
            catch (UnauthorizedAccessException)
            {
                return ApiProblemDetails.FromError(Failure("path.unavailable", "The requested file is not available."));
            }
        }).WithName("StreamTrackAudio");

        return endpoints;
    }

    private static LibraryRoot ToContract(LibraryRootType type) => type switch
    {
        LibraryRootType.Main => LibraryRoot.Main,
        LibraryRootType.Pending => LibraryRoot.Pending,
        LibraryRootType.Remember => LibraryRoot.Remember,
        _ => throw new InvalidOperationException("Sessions is not a Track Management root.")
    };

    private static Error Failure(string code, string message) =>
        new Error(message).WithMetadata("errorCode", code);
}
