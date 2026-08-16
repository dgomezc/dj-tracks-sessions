using DjTrackSessions.Domain.Jobs;

namespace DjTrackSessions.Domain.Notifications;

public sealed class JobNotification
{
    private JobNotification()
    {
    }

    private JobNotification(
        int jobId,
        Guid jobIdentifier,
        JobNotificationKind kind,
        string title,
        string message,
        DateTimeOffset createdAtUtc)
    {
        JobId = jobId;
        JobIdentifier = jobIdentifier;
        Kind = kind;
        Title = title;
        Message = message;
        CreatedAtUtc = createdAtUtc;
    }

    public int Id { get; private set; }

    public int JobId { get; private set; }

    public Guid JobIdentifier { get; private set; }

    public JobNotificationKind Kind { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static JobNotification CreateCompleted(Job job, string title, string message, DateTimeOffset createdAtUtc)
    {
        return Create(job, JobNotificationKind.Completed, title, message, createdAtUtc);
    }

    public static JobNotification CreateFailed(Job job, string title, string message, DateTimeOffset createdAtUtc)
    {
        return Create(job, JobNotificationKind.Failed, title, message, createdAtUtc);
    }

    private static JobNotification Create(
        Job job,
        JobNotificationKind kind,
        string title,
        string message,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(job);

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("A notification title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("A notification message is required.", nameof(message));
        }

        return new JobNotification(
            job.Id,
            job.Identifier,
            kind,
            title.Trim(),
            message.Trim(),
            createdAtUtc);
    }
}
