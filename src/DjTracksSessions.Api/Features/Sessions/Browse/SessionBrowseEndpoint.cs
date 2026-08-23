using DjTracksSessions.Api;
using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using FluentResults;

namespace DjTracksSessions.Api.Features.Sessions.Browse;

public static class SessionBrowseEndpoint
{
    public static IEndpointRouteBuilder MapSessionBrowse(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sessions/browse", (string? path, IConfiguration configuration) =>
        {
            if (path is not null && Path.IsPathFullyQualified(path))
            {
                return ApiProblemDetails.FromError(Failure("path.outside_root", "Only relative paths are accepted."));
            }

            var policies = LibraryRootConfiguration.Load(configuration);
            if (policies.IsFailed)
            {
                return ApiProblemDetails.FromResult(Result.Fail<SessionBrowseResponse>(policies.Errors), Results.Ok);
            }

            var policy = policies.Value.SingleOrDefault(item => item.Type == LibraryRootType.Sessions);
            if (policy is null)
            {
                return ApiProblemDetails.FromError(Failure("sessions.root_unavailable", "The Sessions root is not available."));
            }

            try
            {
                return Results.Ok(new DirectSessionBrowser().Browse(policy, path ?? string.Empty));
            }
            catch (SessionBrowseException exception)
            {
                return ApiProblemDetails.FromError(Failure(exception.Code, exception.Message));
            }
        }).WithName("BrowseSessionsFilesystem");

        return endpoints;
    }

    private static Error Failure(string code, string message) => new Error(message).WithMetadata("errorCode", code);
}
