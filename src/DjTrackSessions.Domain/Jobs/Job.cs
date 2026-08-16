namespace DjTrackSessions.Domain.Jobs;

public sealed class Job
{
    private Job()
    {
    }

    private Job(Guid identifier, DateTimeOffset createdAtUtc)
    {
        Identifier = identifier;
        CreatedAtUtc = createdAtUtc;
        State = JobState.Queued;
        Progress = 0;
    }

    public int Id { get; private set; }

    public Guid Identifier { get; private set; }

    public JobState State { get; private set; }

    public int Progress { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public string? CompletionMessage { get; private set; }

    public string? ErrorCode { get; private set; }

    public string? ErrorMessage { get; private set; }

    public static Job Create(Guid identifier, DateTimeOffset createdAtUtc)
    {
        if (identifier == Guid.Empty)
        {
            throw new ArgumentException("A job identifier is required.", nameof(identifier));
        }

        return new Job(identifier, createdAtUtc);
    }

    public void Start(DateTimeOffset startedAtUtc)
    {
        EnsureState(JobState.Queued);

        State = JobState.Running;
        StartedAtUtc = startedAtUtc;
    }

    public void ReportProgress(int progress)
    {
        if (progress is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(progress), "Progress must be between 0 and 100.");
        }

        Progress = progress;
        if (State == JobState.Queued)
        {
            State = JobState.Running;
        }
    }

    public void Complete(DateTimeOffset completedAtUtc, string? message = null)
    {
        State = JobState.Completed;
        Progress = 100;
        CompletedAtUtc = completedAtUtc;
        CompletionMessage = Normalize(message);
        ErrorCode = null;
        ErrorMessage = null;
    }

    public void Fail(DateTimeOffset completedAtUtc, string errorCode, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException("An error code is required.", nameof(errorCode));
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("An error message is required.", nameof(errorMessage));
        }

        State = JobState.Failed;
        CompletedAtUtc = completedAtUtc;
        ErrorCode = errorCode.Trim();
        ErrorMessage = errorMessage.Trim();
        CompletionMessage = null;
    }

    private void EnsureState(JobState expectedState)
    {
        if (State != expectedState)
        {
            throw new InvalidOperationException($"Job is already {State.ToString().ToLowerInvariant()}.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
