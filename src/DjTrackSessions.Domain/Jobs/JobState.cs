namespace DjTrackSessions.Domain.Jobs;

public enum JobState
{
    Queued = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}
