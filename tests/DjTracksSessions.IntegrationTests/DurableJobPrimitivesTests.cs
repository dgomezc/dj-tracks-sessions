using DjTrackSessions.Domain.Jobs;
using DjTrackSessions.Domain.Notifications;
using DjTrackSessions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class DurableJobPrimitivesTests
{
    [Fact]
    public void Application_db_context_exposes_durable_job_and_notification_tables()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=placeholder;Database=placeholder;Username=placeholder;Password=placeholder")
            .Options;

        using var context = new ApplicationDbContext(options);

        var jobEntity = context.Model.FindEntityType(typeof(Job));
        var notificationEntity = context.Model.FindEntityType(typeof(JobNotification));

        Assert.NotNull(jobEntity);
        Assert.NotNull(notificationEntity);
        Assert.Equal("jobs", jobEntity!.GetTableName());
        Assert.Equal("job_notifications", notificationEntity!.GetTableName());
        Assert.True(jobEntity.FindIndex(jobEntity.FindProperty(nameof(Job.Identifier))!)!.IsUnique);
        var foreignKey = notificationEntity.GetForeignKeys().Single();

        Assert.Equal(nameof(JobNotification.JobId), foreignKey.Properties.Single().Name);
        Assert.Same(jobEntity, foreignKey.PrincipalEntityType);
    }

    [Fact]
    public void Job_and_notification_factories_capture_the_expected_primitives()
    {
        var job = Job.Create(Guid.Parse("11111111-1111-1111-1111-111111111111"), DateTimeOffset.Parse("2026-08-16T12:00:00Z"));
        job.ReportProgress(40);
        job.Complete(DateTimeOffset.Parse("2026-08-16T12:05:00Z"), "Finished");

        var notification = JobNotification.CreateCompleted(
            job,
            "Job completed",
            "The job finished successfully.",
            DateTimeOffset.Parse("2026-08-16T12:05:01Z"));

        Assert.Equal(JobState.Completed, job.State);
        Assert.Equal(100, job.Progress);
        Assert.Equal("Finished", job.CompletionMessage);
        Assert.Equal(job.Identifier, notification.JobIdentifier);
        Assert.Equal(JobNotificationKind.Completed, notification.Kind);
        Assert.Equal(job.Id, notification.JobId);
    }
}
