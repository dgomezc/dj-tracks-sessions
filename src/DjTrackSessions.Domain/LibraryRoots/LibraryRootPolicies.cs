namespace DjTrackSessions.Domain.LibraryRoots;

public static class LibraryRootPolicies
{
    public static IReadOnlyList<LibraryRootPolicy> Create(
        string mainPath,
        string pendingPath,
        string rememberPath,
        string sessionsPath)
    {
        const LibraryRootCapability cataloged =
            LibraryRootCapability.IndexAutomatically |
            LibraryRootCapability.BrowseAndPlay |
            LibraryRootCapability.ManualProviderAnalysis |
            LibraryRootCapability.ApprovalRequiredBeforeAnalyzedWrite |
            LibraryRootCapability.ManualMetadataEditing;

        return
        [
            new(LibraryRootType.Main, mainPath, cataloged),
            new(LibraryRootType.Pending, pendingPath, cataloged),
            new(LibraryRootType.Remember, rememberPath, cataloged | LibraryRootCapability.ForceRememberGenre),
            new(LibraryRootType.Sessions, sessionsPath,
                LibraryRootCapability.IndexAutomatically |
                LibraryRootCapability.BrowseAndPlay |
                LibraryRootCapability.ManualMetadataEditing |
                LibraryRootCapability.SeparateSessionCollection)
        ];
    }
}
