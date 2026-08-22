using DjTrackSessions.Domain.Configuration;
using DjTrackSessions.Domain.Catalog;
using DjTrackSessions.Domain.Jobs;
using DjTrackSessions.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace DjTrackSessions.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<SessionCatalogItem> SessionCatalogItems => Set<SessionCatalogItem>();

    public DbSet<ApplicationSetting> ApplicationSettings => Set<ApplicationSetting>();

    public DbSet<Job> Jobs => Set<Job>();

    public DbSet<JobNotification> JobNotifications => Set<JobNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
