using DjTrackSessions.Domain.LibraryRoots;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace DjTrackSessions.Infrastructure.LibraryRoots;

public static class LibraryRootConfiguration
{
    private static readonly (LibraryRootType Type, string Key)[] RootKeys =
    [
        (LibraryRootType.Main, "MAIN_LIBRARY_PATH"),
        (LibraryRootType.Pending, "PENDING_LIBRARY_PATH"),
        (LibraryRootType.Remember, "REMEMBER_LIBRARY_PATH"),
        (LibraryRootType.Sessions, "SESSIONS_LIBRARY_PATH")
    ];

    public static Result<IReadOnlyList<LibraryRootPolicy>> Load(IConfiguration configuration)
    {
        var roots = new List<LibraryRootPolicy>(RootKeys.Length);
        var errors = new List<IError>();

        foreach (var (type, key) in RootKeys)
        {
            var configuredPath = configuration[key];
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                errors.Add(RootError(type, "root.missing", "is not configured."));
                continue;
            }

            if (!LibraryRootPathResolver.TryCanonicalizeDirectory(configuredPath, out var canonicalPath))
            {
                errors.Add(RootError(type, "root.invalid_path", "is not an existing directory."));
                continue;
            }

            roots.Add(new LibraryRootPolicy(type, canonicalPath, CapabilitiesFor(type)));
        }

        if (errors.Count > 0)
        {
            return Result.Fail<IReadOnlyList<LibraryRootPolicy>>(errors);
        }

        for (var left = 0; left < roots.Count; left++)
        {
            for (var right = left + 1; right < roots.Count; right++)
            {
                if (!LibraryRootPathResolver.PathsOverlap(roots[left].CanonicalPath, roots[right].CanonicalPath))
                {
                    continue;
                }

                errors.Add(new Error($"Roots {roots[left].Type} and {roots[right].Type} overlap.")
                    .WithMetadata("errorCode", "root.overlap")
                    .WithMetadata("roots", $"{roots[left].Type},{roots[right].Type}"));
            }
        }

        return errors.Count == 0
            ? Result.Ok<IReadOnlyList<LibraryRootPolicy>>(roots)
            : Result.Fail<IReadOnlyList<LibraryRootPolicy>>(errors);
    }

    private static Error RootError(LibraryRootType type, string code, string detail) =>
        new Error($"Root {type} {detail}")
            .WithMetadata("errorCode", code)
            .WithMetadata("root", type.ToString());

    private static LibraryRootCapability CapabilitiesFor(LibraryRootType type) => type switch
    {
        LibraryRootType.Main or LibraryRootType.Pending =>
            LibraryRootCapability.IndexAutomatically |
            LibraryRootCapability.BrowseAndPlay |
            LibraryRootCapability.ManualProviderAnalysis |
            LibraryRootCapability.ApprovalRequiredBeforeAnalyzedWrite |
            LibraryRootCapability.ManualMetadataEditing,
        LibraryRootType.Remember =>
            LibraryRootCapability.IndexAutomatically |
            LibraryRootCapability.BrowseAndPlay |
            LibraryRootCapability.ManualProviderAnalysis |
            LibraryRootCapability.ApprovalRequiredBeforeAnalyzedWrite |
            LibraryRootCapability.ManualMetadataEditing |
            LibraryRootCapability.ForceRememberGenre,
        LibraryRootType.Sessions =>
            LibraryRootCapability.IndexAutomatically |
            LibraryRootCapability.BrowseAndPlay |
            LibraryRootCapability.ManualMetadataEditing |
            LibraryRootCapability.SeparateSessionCollection,
        _ => LibraryRootCapability.None
    };
}
