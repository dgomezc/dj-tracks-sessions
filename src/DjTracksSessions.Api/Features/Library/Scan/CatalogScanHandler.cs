using DjTracksSessions.Contracts;
using DjTrackSessions.Domain.Catalog;
using DjTrackSessions.Domain.LibraryRoots;
using DjTrackSessions.Infrastructure.LibraryRoots;
using DjTrackSessions.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DjTracksSessions.Api.Features.Library.Scan;

public sealed class CatalogScanHandler(ApplicationDbContext db)
{
    public async Task<Result<CatalogScanResponse>> HandleAsync(
        IReadOnlyList<LibraryRootPolicy> roots,
        CancellationToken cancellationToken)
    {
        var failures = new List<CatalogScanFileFailure>();
        var observed = new HashSet<(LibraryRootType Root, string Path)>();
        var ordinarySucceeded = 0;
        var sessionsSucceeded = 0;

        foreach (var root in roots)
        {
            var scan = new ReadOnlyLibraryScanner().Scan(root);
            failures.AddRange(scan.Issues.Select(issue => new CatalogScanFileFailure(root.Type.ToString(), issue, "scan.issue", "The path could not be inspected.")));

            if (root.Type == LibraryRootType.Sessions)
            {
                foreach (var session in scan.Sessions)
                {
                    var existing = await db.SessionCatalogItems.SingleOrDefaultAsync(item => item.RelativeFolderPath == session.RelativePath, cancellationToken);
                    var item = existing ?? new SessionCatalogItem { RelativeFolderPath = session.RelativePath };
                    item.AudioRelativePaths = session.AudioFiles.Select(file => file.RelativePath).ToArray();
                    item.TracklistRelativePaths = session.TracklistFiles.ToArray();
                    item.ArtworkRelativePaths = session.ArtworkFiles.ToArray();
                    item.Issues = session.Issues.ToArray();
                    item.IsValid = session.IsValid;
                    item.Duration = await SessionDurationAsync(root, session, cancellationToken);
                    item.LastObservedAtUtc = DateTimeOffset.UtcNow;
                    if (existing is null) db.SessionCatalogItems.Add(item);
                    sessionsSucceeded++;
                }

                continue;
            }

            var extraction = new ReadOnlyAudioExtractionCoordinator().Extract(root, scan);
            foreach (var outcome in extraction.Outcomes)
            {
                var key = (root.Type, outcome.File.RelativePath);
                observed.Add(key);
                if (outcome.IsSuccess && outcome.IsHashSuccess)
                {
                    var track = await FindTrackAsync(root.Type, outcome.File.RelativePath, outcome.ContentHash!, observed, cancellationToken);
                    track ??= new CatalogTrack { RootType = root.Type };
                    track.RelativePath = outcome.File.RelativePath;
                    track.Extension = outcome.File.Extension;
                    track.ContentHash = outcome.ContentHash;
                    track.ByteLength = new FileInfo(outcome.File.FullPath).Length;
                    track.LastObservedAtUtc = DateTimeOffset.UtcNow;
                    track.IsMissing = false;
                    track.LastErrorCode = null;
                    track.LastErrorMessage = null;
                    var metadata = ToMetadata(outcome);
                    if (track.Metadata is null)
                    {
                        track.Metadata = metadata;
                    }
                    else
                    {
                        track.Metadata.Title = metadata.Title;
                        track.Metadata.Artists = metadata.Artists;
                        track.Metadata.Album = metadata.Album;
                        track.Metadata.Year = metadata.Year;
                        track.Metadata.Genre = metadata.Genre;
                        track.Metadata.PersonalGenre = metadata.PersonalGenre;
                        track.Metadata.BeatsPerMinute = metadata.BeatsPerMinute;
                        track.Metadata.Key = metadata.Key;
                        track.Metadata.Duration = metadata.Duration;
                        track.Metadata.BitrateKbps = metadata.BitrateKbps;
                        track.Metadata.SampleRateHz = metadata.SampleRateHz;
                    }
                    if (track.Id == 0) db.CatalogTracks.Add(track);
                    ordinarySucceeded++;
                }
                else
                {
                    failures.Add(new CatalogScanFileFailure(root.Type.ToString(), outcome.File.RelativePath,
                        outcome.ErrorCode ?? outcome.HashErrorCode ?? "scan.failed",
                        outcome.ErrorMessage ?? outcome.HashErrorMessage ?? "The file could not be indexed."));
                }
            }
        }

        var existingTracks = await db.CatalogTracks.ToListAsync(cancellationToken);
        var missingMarked = 0;
        foreach (var track in existingTracks.Where(track => roots.Any(root => root.Type == track.RootType)))
        {
            if (observed.Contains((track.RootType, track.RelativePath))) continue;
            if (!track.IsMissing) { track.IsMissing = true; missingMarked++; }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok(new CatalogScanResponse(ordinarySucceeded, sessionsSucceeded, failures, missingMarked));
    }

    private async Task<CatalogTrack?> FindTrackAsync(
        LibraryRootType root,
        string path,
        string hash,
        HashSet<(LibraryRootType Root, string Path)> observed,
        CancellationToken cancellationToken)
    {
        var byPath = await db.CatalogTracks.SingleOrDefaultAsync(track => track.RootType == root && track.RelativePath == path, cancellationToken);
        if (byPath is not null) return byPath;
        var matches = (await db.CatalogTracks
            .Where(track => track.RootType == root && track.ContentHash == hash)
            .ToListAsync(cancellationToken))
            .Where(track => !observed.Contains((track.RootType, track.RelativePath)))
            .ToList();
        return matches.Count == 1 ? matches[0] : null;
    }

    private static CatalogTrackMetadata ToMetadata(DjTrackSessions.Domain.LibraryScanning.AudioExtractionOutcome outcome)
    {
        var tags = outcome.CurrentTags!;
        var technical = outcome.TechnicalProperties!;
        return new()
        {
            Title = tags.Title,
            Artists = string.Join(", ", tags.Artists),
            Album = tags.Album,
            Year = tags.Year,
            Genre = tags.Genres.FirstOrDefault(),
            BeatsPerMinute = tags.BeatsPerMinute,
            Key = tags.InitialKey,
            Duration = technical.Duration,
            BitrateKbps = technical.BitrateKbps,
            SampleRateHz = technical.SampleRateHz
        };
    }

    private static Task<TimeSpan?> SessionDurationAsync(LibraryRootPolicy root, DjTrackSessions.Domain.LibraryScanning.SessionFolderDiscovery session, CancellationToken cancellationToken)
    {
        var audio = session.AudioFiles.FirstOrDefault();
        if (audio is null) return Task.FromResult<TimeSpan?>(null);
        var outcome = new ReadOnlyAudioExtractionCoordinator().Extract(root, new(session.AudioFiles, [session], [])).Outcomes.FirstOrDefault(item => item.File.RelativePath == audio.RelativePath);
        return Task.FromResult(outcome?.TechnicalProperties?.Duration);
    }
}
