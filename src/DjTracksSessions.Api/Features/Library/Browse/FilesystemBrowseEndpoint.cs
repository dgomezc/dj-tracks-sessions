using DjTracksSessions.Api;
using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using FluentResults;

namespace DjTracksSessions.Api.Features.Library.Browse;

public static class FilesystemBrowseEndpoint
{
    public static IEndpointRouteBuilder MapFilesystemBrowse(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/library/browse", (LibraryRoot? root, string? path, IConfiguration configuration) =>
        {
            if (root is null)
            {
                return ApiProblemDetails.FromResult(Result.Fail<FilesystemBrowseResponse>(Failure("browse.root_required", "A Track Management root is required.")), Results.Ok);
            }

            if (path is not null && Path.IsPathFullyQualified(path))
            {
                return ApiProblemDetails.FromResult(Result.Fail<FilesystemBrowseResponse>(Failure("path.outside_root", "Only relative paths are accepted.")), Results.Ok);
            }

            var policies = LibraryRootConfiguration.LoadTrackManagement(configuration);
            if (policies.IsFailed)
            {
                return ApiProblemDetails.FromResult(Result.Fail<FilesystemBrowseResponse>(policies.Errors), Results.Ok);
            }

            var policy = policies.Value.SingleOrDefault(item => ToContract(item.Type) == root.Value);
            if (policy is null)
            {
                return ApiProblemDetails.FromResult(Result.Fail<FilesystemBrowseResponse>(Failure("browse.root_invalid", "The requested root is not available for Track Management.")), Results.Ok);
            }

            try
            {
                return Results.Ok(new DirectLibraryBrowser().Browse(policy, path ?? string.Empty));
            }
            catch (FilesystemBrowseException exception)
            {
                return ApiProblemDetails.FromResult(Result.Fail<FilesystemBrowseResponse>(Failure(exception.Code, exception.Message)), Results.Ok);
            }
        }).WithName("BrowseTrackManagementFilesystem");
        return endpoints;
    }

    private static LibraryRoot ToContract(LibraryRootType type) => type switch
    {
        LibraryRootType.Main => LibraryRoot.Main,
        LibraryRootType.Pending => LibraryRoot.Pending,
        _ => LibraryRoot.Remember
    };

    private static Error Failure(string code, string message) => new Error(message).WithMetadata("errorCode", code);
}
