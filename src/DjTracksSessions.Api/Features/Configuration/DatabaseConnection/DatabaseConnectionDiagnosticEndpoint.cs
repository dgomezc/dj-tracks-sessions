using DjTrackSessions.Infrastructure.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DjTracksSessions.Api.Features.Configuration.DatabaseConnection;

public static class DatabaseConnectionDiagnosticEndpoint
{
    public static IEndpointRouteBuilder MapDatabaseConnectionDiagnostic(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/configuration/database-connection", async (
            IDatabaseHealthProbe probe,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await probe.CheckAsync(cancellationToken);
                var isConnected = result.Status == HealthStatus.Healthy;

                return Results.Json(
                    new DatabaseConnectionDiagnosticResponse(isConnected ? "connected" : "unavailable"),
                    statusCode: isConnected ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                return Results.Json(
                    new DatabaseConnectionDiagnosticResponse("unavailable"),
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        })
            .WithName("GetDatabaseConnectionDiagnostic")
            .WithSummary("Check database connectivity")
            .WithDescription("Returns only whether the configured database probe is connected or unavailable.");

        return endpoints;
    }
}
