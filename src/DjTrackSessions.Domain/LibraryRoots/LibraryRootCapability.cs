namespace DjTrackSessions.Domain.LibraryRoots;

[Flags]
public enum LibraryRootCapability
{
    None = 0,
    IndexAutomatically = 1 << 0,
    BrowseAndPlay = 1 << 1,
    ManualProviderAnalysis = 1 << 2,
    ApprovalRequiredBeforeAnalyzedWrite = 1 << 3,
    ManualMetadataEditing = 1 << 4,
    SeparateSessionCollection = 1 << 5,
    ForceRememberGenre = 1 << 6
}
