using DjTracksSessions.Api;
using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using FluentResults;

namespace DjTracksSessions.Api.Features.Sessions.Metadata;

public static class SessionMetadataEndpoint
{
    public static IEndpointRouteBuilder MapSessionMetadata(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sessions/detail", (string? path, IConfiguration configuration) =>
            WithRoot(configuration, path ?? string.Empty, (root, relativePath) => new DirectSessionBrowser().Detail(root, relativePath)));
        endpoints.MapPost("/sessions/edit/preview", async (SessionEditRequest request, IConfiguration configuration, CancellationToken cancellationToken) =>
            await ExecuteAsync(request, configuration, cancellationToken, (writer, root) => writer.PreviewAsync(root, request.Identity, request.Update, cancellationToken)));
        endpoints.MapPost("/sessions/edit", async (SessionEditRequest request, IConfiguration configuration, CancellationToken cancellationToken) =>
            await ExecuteAsync(request, configuration, cancellationToken, (writer, root) => writer.ApplyAsync(root, request.Identity, request.Update, cancellationToken)));
        return endpoints;
    }

    private static async Task<IResult> ExecuteAsync<T>(SessionEditRequest request, IConfiguration configuration, CancellationToken cancellationToken, Func<SessionMetadataWriter, LibraryRootPolicy, Task<Result<T>>> action)
    {
        var root = Root(configuration);
        if (root.IsFailed) return ApiProblemDetails.FromResult(Result.Fail<T>(root.Errors), Results.Ok);
        try { return ApiProblemDetails.FromResult(await action(new SessionMetadataWriter(), root.Value), Results.Ok); }
        catch (SessionBrowseException exception) { return ApiProblemDetails.FromError(Failure(exception.Code, exception.Message)); }
    }

    private static IResult WithRoot(IConfiguration configuration, string path, Func<LibraryRootPolicy, string, SessionDetail> action)
    {
        var root = Root(configuration);
        if (root.IsFailed) return ApiProblemDetails.FromResult(Result.Fail<SessionDetail>(root.Errors), Results.Ok);
        try { return Results.Ok(action(root.Value, path)); }
        catch (SessionBrowseException exception) { return ApiProblemDetails.FromError(Failure(exception.Code, exception.Message)); }
    }

    private static Result<LibraryRootPolicy> Root(IConfiguration configuration) =>
        LibraryRootConfiguration.Load(configuration).Bind(roots => roots.SingleOrDefault(root => root.Type == LibraryRootType.Sessions) is { } session
            ? Result.Ok(session) : Result.Fail<LibraryRootPolicy>(Failure("sessions.root_unavailable", "The Sessions root is not available.")));

    private static Error Failure(string code, string message) => new Error(message).WithMetadata("errorCode", code);
}
