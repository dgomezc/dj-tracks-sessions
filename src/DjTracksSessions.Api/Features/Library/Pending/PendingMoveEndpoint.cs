using DjTracksSessions.Api;
using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using FluentResults;

namespace DjTracksSessions.Api.Features.Library.Pending;

public static class PendingMoveEndpoint
{
    public static IEndpointRouteBuilder MapPendingMove(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/library/pending/move-preview", (TrackIdentity identity, IConfiguration configuration) =>
        {
            var result = PendingMoveService.Preview(identity, configuration);
            return result.IsSuccess ? Results.Ok(result.Value) : ApiProblemDetails.FromResult(Result.Fail<PendingMovePreview>(result.Errors), Results.Ok);
        });
        endpoints.MapPost("/library/pending/move", (PendingMoveRequest request, IConfiguration configuration) =>
        {
            var result = PendingMoveService.Move(request.Identity, request.Confirmation, configuration);
            return result.IsSuccess ? Results.Ok(result.Value) : ApiProblemDetails.FromResult(Result.Fail<FilesystemTrackDetail>(result.Errors), Results.Ok);
        });
        return endpoints;
    }
}

internal static class PendingMoveService
{
    public static Result<PendingMovePreview> Preview(TrackIdentity identity, IConfiguration configuration)
    {
        if (identity.Root != LibraryRoot.Pending) return Failure<PendingMovePreview>("move.root_forbidden", "Only Pending tracks can be moved.");
        var roots = LibraryRootConfiguration.LoadTrackManagement(configuration);
        if (roots.IsFailed) return Result.Fail<PendingMovePreview>(roots.Errors);
        var sourceRoot = roots.Value.Single(item => item.Type == LibraryRootType.Pending);
        var destinationRoot = roots.Value.Single(item => item.Type == LibraryRootType.Main);
        var source = LibraryRootPathResolver.ResolveExistingPath(sourceRoot, identity.RelativePath);
        if (source.IsFailed) return Result.Fail<PendingMovePreview>(source.Errors);
        var tags = new TagLibSharpAudioMetadataReader().Read(sourceRoot, new(identity.RelativePath, identity.RelativePath, Path.GetExtension(identity.RelativePath))).CurrentTags;
        if (tags is null) return Failure<PendingMovePreview>("tag.read_failed", "The pending track tags could not be read.");
        var genre = tags.PersonalGenre;
        if (string.IsNullOrWhiteSpace(genre)) return Failure<PendingMovePreview>("move.genre_required", "Pending track requires an approved PersonalGenre before movement.");
        var year = tags.Year ?? (uint)DateTime.UtcNow.Year;
        var filename = Sanitize($"{string.Join(", ", tags.Artists.DefaultIfEmpty("Unknown Artist"))} - {tags.Title ?? Path.GetFileNameWithoutExtension(identity.RelativePath)}{Path.GetExtension(identity.RelativePath)}");
        var directory = Path.Combine(year.ToString(), $"House - {genre}");
        var relativeDestination = Path.Combine(directory, filename).Replace(Path.DirectorySeparatorChar, '/');
        var destination = LibraryRootPathResolver.ResolveExistingPath(destinationRoot, relativeDestination);
        var collision = destination.IsSuccess && File.Exists(destination.Value);
        if (destination.IsFailed && destination.Errors[0].Metadata.TryGetValue("errorCode", out var code) && !Equals(code, "path.not_found")) return Result.Fail<PendingMovePreview>(destination.Errors);
        return Result.Ok(new PendingMovePreview(identity, directory.Replace(Path.DirectorySeparatorChar, '/'), relativeDestination, filename, collision, !collision, collision ? "file.collision" : null, collision ? "The destination file already exists." : null));
    }

    public static Result<FilesystemTrackDetail> Move(TrackIdentity identity, PendingMoveConfirmation confirmation, IConfiguration configuration)
    {
        if (!confirmation.Confirmed) return Failure<FilesystemTrackDetail>("move.confirmation_required", "Movement confirmation is required.");
        var preview = Preview(identity, configuration);
        if (preview.IsFailed) return Result.Fail<FilesystemTrackDetail>(preview.Errors);
        if (!string.Equals(confirmation.SourceRelativePath, identity.RelativePath, StringComparison.Ordinal) ||
            !string.Equals(confirmation.DestinationRelativePath, preview.Value.DestinationRelativePath, StringComparison.Ordinal) ||
            !string.Equals(confirmation.FinalFileName, preview.Value.FinalFileName, StringComparison.Ordinal))
        {
            return Failure<FilesystemTrackDetail>("move.preview_stale", "The movement preview is stale and must be recalculated.");
        }
        var configuredRoots = LibraryRootConfiguration.LoadTrackManagement(configuration);
        if (configuredRoots.IsFailed) return Result.Fail<FilesystemTrackDetail>(configuredRoots.Errors);
        var roots = configuredRoots.Value;
        var sourceRoot = roots.Single(item => item.Type == LibraryRootType.Pending);
        var destinationRoot = roots.Single(item => item.Type == LibraryRootType.Main);
        var source = LibraryRootPathResolver.ResolveExistingPath(sourceRoot, identity.RelativePath);
        if (source.IsFailed) return Result.Fail<FilesystemTrackDetail>(source.Errors);
        var destination = LibraryRootPathResolver.ResolveExistingPath(destinationRoot, preview.Value.DestinationRelativePath);
        if (destination.IsSuccess) return Failure<FilesystemTrackDetail>("file.collision", "The destination file already exists.");
        if (!IsNotFound(destination)) return Result.Fail<FilesystemTrackDetail>(destination.Errors);
        var destinationDirectory = Path.GetDirectoryName(preview.Value.DestinationRelativePath);
        var resolvedDirectory = LibraryRootPathResolver.ResolvePathUnderRoot(destinationRoot, destinationDirectory ?? string.Empty);
        if (resolvedDirectory.IsFailed) return Result.Fail<FilesystemTrackDetail>(resolvedDirectory.Errors);
        var destinationPath = Path.Combine(resolvedDirectory.Value, preview.Value.FinalFileName);
        Directory.CreateDirectory(resolvedDirectory.Value);
        try { File.Move(source.Value, destinationPath); }
        catch (IOException exception) { return Failure<FilesystemTrackDetail>("move.write_failed", exception.Message); }
        if (File.Exists(source.Value) || !File.Exists(destinationPath)) return Failure<FilesystemTrackDetail>("move.verification_failed", "The filesystem move could not be verified.");
        var moved = new TrackIdentity(LibraryRoot.Main, preview.Value.DestinationRelativePath);
        var outcome = new TagLibSharpAudioMetadataReader().Read(destinationRoot, new(moved.RelativePath, moved.RelativePath, Path.GetExtension(moved.RelativePath)));
        return Result.Ok(new FilesystemTrackDetail(moved.Root, moved.RelativePath, Path.GetExtension(moved.RelativePath), outcome.CurrentTags?.Title, outcome.CurrentTags?.Artists ?? [], outcome.CurrentTags?.Album, outcome.CurrentTags?.Genres ?? [], outcome.CurrentTags?.Year, outcome.CurrentTags?.PersonalGenre, outcome.CurrentTags?.InitialKey, outcome.CurrentTags?.BeatsPerMinute, outcome.TechnicalProperties?.Duration, outcome.TechnicalProperties?.BitrateKbps, outcome.TechnicalProperties?.SampleRateHz, outcome.ErrorCode, outcome.ErrorMessage));
    }

    private static string Sanitize(string value) => string.Concat(value.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character)).Trim();
    private static bool IsNotFound(Result<string> result) => result.Errors.FirstOrDefault()?.Metadata.TryGetValue("errorCode", out var code) == true && Equals(code, "path.not_found");
    private static Result<T> Failure<T>(string code, string message) => Result.Fail<T>(new Error(message).WithMetadata("errorCode", code));
}
