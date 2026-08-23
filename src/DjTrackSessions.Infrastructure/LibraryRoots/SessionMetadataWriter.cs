using System.Security.Cryptography;
using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using FluentResults;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Mpeg;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public sealed class SessionMetadataWriter
{
    private readonly DirectSessionBrowser browser;

    public SessionMetadataWriter(DirectSessionBrowser? browser = null) => this.browser = browser ?? new DirectSessionBrowser();

    public Task<Result<SessionEditPreview>> PreviewAsync(LibraryRootPolicy root, SessionIdentity identity, SessionMetadataUpdate update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var before = browser.Detail(root, identity.RelativePath);
        if (before.ErrorCode is not null) return Task.FromResult(Result.Fail<SessionEditPreview>(new Error(before.ErrorMessage ?? "The session metadata could not be read.").WithMetadata("errorCode", before.ErrorCode)));
        var validation = Validate(before, update);
        if (validation.IsFailed) return Task.FromResult(Result.Fail<SessionEditPreview>(validation.Errors));
        var after = before with
        {
            Title = Clean(update.Title), Artists = Split(update.Artists), Album = Clean(update.Album),
            Year = update.Year, Genres = Split(update.Genre), ErrorCode = null, ErrorMessage = null
        };
        return Task.FromResult(Result.Ok(new SessionEditPreview(identity, ".mp3", before, after, true, null, null)));
    }

    public async Task<Result<SessionEditResponse>> ApplyAsync(LibraryRootPolicy root, SessionIdentity identity, SessionMetadataUpdate update, CancellationToken cancellationToken)
    {
        var preview = await PreviewAsync(root, identity, update, cancellationToken);
        if (preview.IsFailed) return Result.Fail<SessionEditResponse>(preview.Errors);
        var source = LibraryRootPathResolver.ResolveExistingPath(root, identity.RelativePath);
        if (source.IsFailed) return Result.Fail<SessionEditResponse>(source.Errors);
        var temporary = source.Value + ".dj-tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            var original = await File.ReadAllBytesAsync(source.Value, cancellationToken);
            var parsed = Mp3File.Read(original);
            if (!parsed.IsSuccess || parsed.File is null) return Failure<SessionEditResponse>("tag.read_failed", "The MP3 file could not be reopened for editing.");
            var file = parsed.File;
            var after = preview.Value.After;
            file.Title = after.Title;
            file.Artist = string.Join(", ", after.Artists);
            file.Album = after.Album;
            file.Year = after.Year?.ToString() ?? string.Empty;
            file.Genre = string.Join(", ", after.Genres);
            await File.WriteAllBytesAsync(temporary, file.Render(original).ToArray(), cancellationToken);

            var verification = Mp3File.Read(await File.ReadAllBytesAsync(temporary, cancellationToken));
            if (!verification.IsSuccess || verification.File is null || verification.File.Title != after.Title ||
                verification.File.Artist != string.Join(", ", after.Artists) || verification.File.Album != after.Album ||
                verification.File.Year != (after.Year?.ToString() ?? string.Empty) || verification.File.Genre != string.Join(", ", after.Genres))
                return Failure<SessionEditResponse>("tag.verification_failed", "The temporary MP3 failed post-write verification.");

            var current = await File.ReadAllBytesAsync(source.Value, cancellationToken);
            if (!SHA256.HashData(original).SequenceEqual(SHA256.HashData(current))) return Failure<SessionEditResponse>("tag.source_changed", "The source MP3 changed while it was being edited.");
            File.Move(temporary, source.Value, true);
            return Result.Ok(new SessionEditResponse(identity, browser.Detail(root, identity.RelativePath)));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or FormatException)
        {
            return Failure<SessionEditResponse>("tag.write_failed", exception.Message);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static Result Validate(SessionDetail before, SessionMetadataUpdate update) =>
        !string.Equals(before.Extension, ".mp3", StringComparison.OrdinalIgnoreCase)
            ? Result.Fail(new Error("Only MP3 text metadata editing is supported for Sessions.").WithMetadata("errorCode", "tag.unsupported"))
            : update.Year > 9999 ? Result.Fail(new Error("Year must be a valid four-digit value.").WithMetadata("errorCode", "metadata.year_invalid")) : Result.Ok();

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static IReadOnlyList<string> Split(string? value) => Clean(value)?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
    private static Result<T> Failure<T>(string code, string message) => Result.Fail<T>(new Error(message).WithMetadata("errorCode", code));
}
