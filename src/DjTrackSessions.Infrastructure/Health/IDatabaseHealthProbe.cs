using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DjTrackSessions.Infrastructure.Health;

public interface IDatabaseHealthProbe
{
    Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken);
}
