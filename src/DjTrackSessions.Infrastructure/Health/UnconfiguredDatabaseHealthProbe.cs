using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DjTrackSessions.Infrastructure.Health;

public sealed class UnconfiguredDatabaseHealthProbe : IDatabaseHealthProbe
{
    public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(HealthCheckResult.Unhealthy("PostgreSQL connection is not configured."));
    }
}
