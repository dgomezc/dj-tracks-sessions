using DjTrackSessions.Domain.Jobs;
using DjTrackSessions.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DjTrackSessions.Infrastructure.Persistence.Configurations;

public sealed class JobNotificationConfiguration : IEntityTypeConfiguration<JobNotification>
{
    public void Configure(EntityTypeBuilder<JobNotification> builder)
    {
        builder.ToTable("job_notifications");
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.JobIdentifier)
            .IsRequired();
        builder.Property(notification => notification.Kind)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(notification => notification.Title)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(notification => notification.Message)
            .HasMaxLength(2000)
            .IsRequired();
        builder.Property(notification => notification.CreatedAtUtc)
            .IsRequired();
        builder.HasOne<Job>()
            .WithMany()
            .HasForeignKey(notification => notification.JobId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(notification => notification.JobId);
    }
}
