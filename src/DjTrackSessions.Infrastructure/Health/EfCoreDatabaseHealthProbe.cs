using DjTrackSessions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DjTrackSessions.Infrastructure.Health;

public sealed class EfCoreDatabaseHealthProbe(ApplicationDbContext dbContext) : IDatabaseHealthProbe
{
    public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("PostgreSQL connection succeeded.")
            : HealthCheckResult.Unhealthy("PostgreSQL connection failed.");
    }
}
