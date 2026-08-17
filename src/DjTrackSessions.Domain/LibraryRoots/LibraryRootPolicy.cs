namespace DjTrackSessions.Domain.LibraryRoots;

public sealed record LibraryRootPolicy(
    LibraryRootType Type,
    string CanonicalPath,
    LibraryRootCapability Capabilities);
