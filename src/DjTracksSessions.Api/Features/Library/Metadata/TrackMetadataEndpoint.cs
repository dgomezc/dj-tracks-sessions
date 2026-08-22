using DjTracksSessions.Api;
using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using FluentResults;

namespace DjTracksSessions.Api.Features.Library.Metadata;

public static class TrackMetadataEndpoint
{
    public static IEndpointRouteBuilder MapTrackMetadata(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/library/tracks/edit/preview", async (TrackEditRequest request, IConfiguration configuration, CancellationToken cancellationToken) =>
        {
            var result = await Mp3MetadataWriter.PreviewAsync(request.Identity, request.Update, configuration, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : ApiProblemDetails.FromResult(Result.Fail<TrackEditPreview>(result.Errors), Results.Ok);
        });
        endpoints.MapPost("/library/tracks/edit", async (TrackEditRequest request, IConfiguration configuration, CancellationToken cancellationToken) =>
        {
            var result = await Mp3MetadataWriter.ApplyAsync(request.Identity, request.Update, configuration, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : ApiProblemDetails.FromResult(Result.Fail<TrackEditResponse>(result.Errors), Results.Ok);
        });
        return endpoints;
    }
}

internal static class Mp3MetadataWriter
{
    private static readonly string[] PersonalGenres = ["Day Instrumental", "Day Vocal", "Night Instrumental", "Night Vocal", "TechnoHouse", "Tribal", "Remember"];

    public static async Task<Result<TrackEditPreview>> PreviewAsync(TrackIdentity identity, TrackMetadataUpdate update, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var root = ResolveRoot(identity, configuration);
        if (root.IsFailed) return Result.Fail<TrackEditPreview>(root.Errors);
        var source = LibraryRootPathResolver.ResolveExistingPath(root.Value, identity.RelativePath);
        if (source.IsFailed) return Result.Fail<TrackEditPreview>(source.Errors);
        var current = Read(root.Value, identity);
        var validation = Validate(root.Value, current, update);
        if (validation.IsFailed) return Result.Fail<TrackEditPreview>(validation.Errors);
        var after = current with { Title = Clean(update.Title), Artists = Split(update.Artists), Album = Clean(update.Album), Year = update.Year, Genres = Split(update.Genre), PersonalGenre = Clean(update.PersonalGenre) };
        return Result.Ok(new TrackEditPreview(identity, ".mp3", current, after, true, null, null));
    }

    public static async Task<Result<TrackEditResponse>> ApplyAsync(TrackIdentity identity, TrackMetadataUpdate update, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var preview = await PreviewAsync(identity, update, configuration, cancellationToken);
        if (preview.IsFailed) return Result.Fail<TrackEditResponse>(preview.Errors);
        var root = ResolveRoot(identity, configuration).Value;
        var source = LibraryRootPathResolver.ResolveExistingPath(root, identity.RelativePath);
        if (source.IsFailed) return Result.Fail<TrackEditResponse>(source.Errors);
        var temporary = source.Value + ".dj-tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            var original = await File.ReadAllBytesAsync(source.Value, cancellationToken);
            var parsed = TagLibSharp2.Mpeg.Mp3File.Read(original);
            if (!parsed.IsSuccess || parsed.File is null) return Failure<TrackEditResponse>("tag.read_failed", "The MP3 file could not be reopened for editing.");
            var file = parsed.File;
            var after = preview.Value.After;
            file.Title = after.Title;
            file.Artist = string.Join(", ", after.Artists);
            file.Album = after.Album;
            file.Year = after.Year?.ToString() ?? string.Empty;
            file.Genre = string.Join(", ", after.Genres);
            file.Id3v2Tag ??= new TagLibSharp2.Id3.Id3v2.Id3v2Tag();
            file.Id3v2Tag.SetUserText("PERSONAL_GENRE", after.PersonalGenre ?? string.Empty);
            await File.WriteAllBytesAsync(temporary, file.Render(original).ToArray(), cancellationToken);
            var verification = TagLibSharp2.Mpeg.Mp3File.Read(await File.ReadAllBytesAsync(temporary, cancellationToken));
            if (!verification.IsSuccess || verification.File is null || verification.File.Title != after.Title || verification.File.Artist != string.Join(", ", after.Artists) || verification.File.Id3v2Tag?.GetUserText("PERSONAL_GENRE") != after.PersonalGenre) return Failure<TrackEditResponse>("tag.verification_failed", "The temporary MP3 failed post-write verification.");
            var currentSource = await File.ReadAllBytesAsync(source.Value, cancellationToken);
            var originalHash = System.Security.Cryptography.SHA256.HashData(original);
            var currentHash = System.Security.Cryptography.SHA256.HashData(currentSource);
            if (!originalHash.SequenceEqual(currentHash)) return Failure<TrackEditResponse>("tag.source_changed", "The source MP3 changed while it was being edited.");
            File.Move(temporary, source.Value, true);
            return Result.Ok(new TrackEditResponse(identity, Read(root, identity)));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException) { return Failure<TrackEditResponse>("tag.write_failed", exception.Message); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static Result<LibraryRootPolicy> ResolveRoot(TrackIdentity identity, IConfiguration configuration)
    {
        var root = LibraryRootConfiguration.LoadTrackManagement(configuration).ValueOrDefault?.SingleOrDefault(item => (LibraryRoot)item.Type == identity.Root);
        return root is null ? Result.Fail<LibraryRootPolicy>(new Error("The configured root is unavailable.").WithMetadata("errorCode", "root.unavailable")) : Result.Ok(root);
    }

    private static FilesystemTrackDetail Read(LibraryRootPolicy root, TrackIdentity identity)
    {
        var outcome = new TagLibSharpAudioMetadataReader().Read(root, new(identity.RelativePath, identity.RelativePath, Path.GetExtension(identity.RelativePath)));
        return new(identity.Root, identity.RelativePath, Path.GetExtension(identity.RelativePath), outcome.CurrentTags?.Title, outcome.CurrentTags?.Artists ?? [], outcome.CurrentTags?.Album, outcome.CurrentTags?.Genres ?? [], outcome.CurrentTags?.Year, outcome.CurrentTags?.PersonalGenre, outcome.CurrentTags?.InitialKey, outcome.CurrentTags?.BeatsPerMinute, outcome.TechnicalProperties?.Duration, outcome.TechnicalProperties?.BitrateKbps, outcome.TechnicalProperties?.SampleRateHz, outcome.ErrorCode, outcome.ErrorMessage);
    }

    private static Result Validate(LibraryRootPolicy root, FilesystemTrackDetail current, TrackMetadataUpdate update)
    {
        if (!string.Equals(current.Extension, ".mp3", StringComparison.OrdinalIgnoreCase)) return Result.Fail(new Error("Only MP3 text metadata editing is supported.").WithMetadata("errorCode", "tag.unsupported"));
        if (root.Type == LibraryRootType.Remember && !string.Equals(update.PersonalGenre, "Remember", StringComparison.Ordinal)) return Result.Fail(new Error("Remember tracks must use PersonalGenre Remember.").WithMetadata("errorCode", "metadata.remember_genre_required"));
        return string.IsNullOrWhiteSpace(update.PersonalGenre) || !PersonalGenres.Contains(update.PersonalGenre, StringComparer.Ordinal) ? Result.Fail(new Error("PersonalGenre is not supported.").WithMetadata("errorCode", "metadata.personal_genre_invalid")) : Result.Ok();
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static IReadOnlyList<string> Split(string? value) => Clean(value)?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
    private static Result<T> Failure<T>(string code, string message) => Result.Fail<T>(new Error(message).WithMetadata("errorCode", code));
}
