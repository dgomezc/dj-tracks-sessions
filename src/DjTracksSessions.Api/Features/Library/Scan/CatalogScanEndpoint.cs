using DjTracksSessions.Api.Validation;
using DjTracksSessions.Contracts;
using DjTrackSessions.Infrastructure.LibraryRoots;
using DjTrackSessions.Infrastructure.Persistence;
using FluentResults;
using FluentValidation;

namespace DjTracksSessions.Api.Features.Library.Scan;

public static class CatalogScanEndpoint
{
    public static IEndpointRouteBuilder MapCatalogScan(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/library/scan", async (
            CatalogScanRequest request,
            IValidator<CatalogScanRequest> validator,
            IConfiguration configuration,
            ApplicationDbContext db,
            CancellationToken cancellationToken) =>
        {
            var validation = await ApiValidation.ValidateAsync(request, validator, cancellationToken);
            if (validation.IsFailed)
            {
                return ApiProblemDetails.FromResult(validation, _ => Results.Ok());
            }

            var roots = LibraryRootConfiguration.Load(configuration);
            if (roots.IsFailed)
            {
                return ApiProblemDetails.FromResult(
                    Result.Fail<CatalogScanResponse>(roots.Errors),
                    value => Results.Ok(value));
            }

            var result = await new CatalogScanHandler(db).HandleAsync(roots.Value, cancellationToken);
            return ApiProblemDetails.FromResult(result, value => Results.Ok(value));
        })
            .WithName("ScanCatalog")
            .WithSummary("Run an explicit read-only catalog scan")
            .WithDescription("Scans the four configured roots synchronously without modifying source files.");

        return endpoints;
    }
}
