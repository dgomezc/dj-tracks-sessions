using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DjTrackSessions.Infrastructure.Health;

public sealed class DatabaseHealthCheck(IDatabaseHealthProbe probe) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return probe.CheckAsync(cancellationToken);
    }
}
