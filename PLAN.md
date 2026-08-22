# DJ Tracks & Sessions Implementation Plan

This document is the executable source of truth for product scope, architecture, delivery order, and acceptance criteria. `b1f4df6` completed Phase 2A Work Unit 1, explicit read-only scan persistence. The immediate stage is the Track Management screen; delivery then proceeds strictly by complete screens: Track Management, Player, Sessions, then remaining screens.

An implementation agent must complete phases in order. Within a phase, deliver one vertical work unit at a time with its tests. A screen is not complete until its API behavior, UI states, safety flow, and acceptance evidence are present; do not start the next screen until the current screen exit criteria pass.

## 1. Product Outcome

Build a Spanish-language, single-user web application that runs in Docker on a Linux x86-64 NAS and manages a personal House/electronic music collection.

The product has three clearly separated experiences:

1. **Explorer / Player** for browsing, searching, playing, and organizing cataloged tracks.
2. **Analyzer / Tagger** for identifying tracks, reviewing proposals, writing metadata, and processing pending files.
3. **Sessions** for browsing and playing personal DJ sessions with associated TXT tracklists.

## 2. Non-Goals

- Desktop, mobile, Tauri, or Electron applications.
- Multi-user accounts, authorization, or internet exposure in the MVP.
- Automatic Beatport scraping.
- Automatic deletion of any file.
- Automatically moving files already in Main or Remember.
- Automatically analyzing Sessions.
- Statistics dashboards.
- Managing lyrics, CUE files, or external artwork.
- Session track timestamps or tracklist editing in the MVP.

## 3. Runtime Configuration

Docker Compose must configure these host paths as read/write mounts:

| Environment setting | Container path | Policy |
|---|---|---|
| `MAIN_LIBRARY_PATH` | `/music/main` | Cataloged tracks; reanalysis by explicit request and approval |
| `PENDING_LIBRARY_PATH` | `/music/pending` | Inbox; detect automatically, analyze manually, move after approval and confirmation |
| `REMEMBER_LIBRARY_PATH` | `/music/remember` | Analyze/edit without moving; force `PersonalGenre=Remember` |
| `SESSIONS_LIBRARY_PATH` | `/music/sessions` | Manual metadata only; separate player and TXT tracklists |
| Application data | `/app/data` | Waveform cache and non-database runtime data |
| PostgreSQL connection | External NAS-local secret/configuration | Catalog, history, job state, playlists, notifications, and configuration |

Also configure:

- MusicBrainz application identity and contact.
- Discogs token.
- AcoustID application key.
- NAS path to Windows/UNC translation profiles for playlist exports.
- Scan/reconciliation interval.
- Analysis concurrency, defaulting to one.
- PostgreSQL connection string from a NAS-local secret/configuration source for containers, or .NET User Secrets for local API development.

Secrets must come from environment variables or Docker secrets and must never be committed.

PostgreSQL requirements:

- The NAS PostgreSQL instance is the development database. Compose must not deploy, start, mount, or own PostgreSQL containers or volumes.
- Local API development in the `Development` environment reads `ConnectionStrings:Postgres` from .NET User Secrets. Configure containers with the external Npgsql connection string supplied as `ConnectionStrings__Postgres` through an ignored environment file or secret source; User Secrets are not available inside containers.
- Provision a separate production database only for the future production deployment.
- Use Entity Framework Core migrations as the only schema evolution mechanism.
- Apply migrations through an explicit deployment/startup step that fails visibly; never silently create or recreate the database.
- Use PostgreSQL constraints and transactions for invariants that cross persisted records.
- Test database behavior against the supported PostgreSQL major version through Testcontainers for .NET.

## 4. Target Architecture

```text
Desktop browser
  |
  +-- DjTracksSessions.Web (Blazor Interactive Server)
          |
          +-- generated/typed HTTP client
                  |
                  +-- DjTracksSessions.Api
                        +-- Vertical slices
                        +-- Background job worker
                        +-- DjTrackSessions.Domain (entities, value objects, invariants)
                        +-- DjTrackSessions.Infrastructure (EF Core/Npgsql, migrations, adapters)
```

### 4.1 Project Layout

Create the minimum useful projects:

```text
src/
  DjTracksSessions.Api/
    Features/
      Library/
      Pending/
      Matching/
      Metadata/
      Playback/
      Playlists/
      Duplicates/
      History/
      Sessions/
  DjTrackSessions.Domain/
    Entities and value objects
    Domain invariants
  DjTrackSessions.Infrastructure/
    EF Core Code First DbContext and Fluent configurations
    EF Core migrations
    Database, filesystem, provider, image, and audio-tool adapters
  DjTracksSessions.Web/
  DjTracksSessions.Contracts/
tests/
  DjTracksSessions.UnitTests/
  DjTracksSessions.IntegrationTests/
```

`DjTracksSessions.Api` owns the vertical-slice `Features` directories. Each feature contains its endpoint, queries, commands, validations, mappers, handlers, and behavior tests; it uses `DjTrackSessions.Domain` for entities and invariants and `DjTrackSessions.Infrastructure` adapters. Do not move persistence or external implementations into the API, turn slices into generic horizontal folders, or create further layer projects without a real dependency boundary.

### 4.2 Slice Contract

Each command/query slice should contain:

- Endpoint and request/route contract.
- Query or command.
- FluentValidation validator.
- Handler.
- FluentResults success/failure result.
- Slice-specific mapping.
- Behavior tests.

Use stable error codes such as `track.not_found`, `path.outside_root`, `match.ambiguous`, `file.collision`, and `tag.unsupported`.

### 4.3 Background Work

Long operations must not run in a Blazor circuit or HTTP request:

- Root scans and reconciliations.
- Hashing and fingerprinting.
- Provider lookups.
- BPM/key analysis.
- Waveform generation.
- Bulk tag operations.
- History cleanup.

Persist job status in PostgreSQL through Entity Framework Core. Expose progress and completion through API polling or server notifications. A restart must not silently lose queued/active work; interrupted items become retryable or failed with a reason.

## 5. Core Data Model

The exact EF entities may differ, but the persisted concepts must include:

| Concept | Required responsibility |
|---|---|
| `LibraryRoot` | Root type, canonical path, and allowed capabilities |
| `AudioTrack` | Stable ID, current path, root, technical properties, hashes, status |
| `TrackMetadata` | Structured current provider/personal/effective metadata |
| `MetadataProvenance` | Source, confidence, provider ID, and retrieval date per field |
| `Artwork` | Current normalized image metadata/cache reference |
| `AnalysisRun` | Input identity, state, tool versions, results, and failures |
| `MatchCandidate` | Provider candidate, score, evidence, and selection state |
| `ChangeProposal` | Before/after values requiring approval |
| `ChangeHistory` | Reversible before state, operation, timestamp, and expiry |
| `Playlist` | Manual ordered entries or smart rule definition |
| `PlaybackSession` | Queue, shuffle state, current item, and last position |
| `DuplicateGroup` | Exact/possible duplicate evidence and resolution state |
| `DjSession` | Separate session metadata, audio identity, and tracklist path |
| `Job` | Durable work state and progress |
| `Notification` | Persistent completion/error notification |

Use an optimistic concurrency token for mutating records and reject stale approvals.

## 6. Metadata Rules

### 6.1 Structured Values

Store these separately rather than parsing display strings later:

- Ordered primary artists.
- Base title.
- Mix name.
- Remixers.
- Provider year and personal year.
- Provider genre and PersonalGenre.
- Provider and locally analyzed BPM/key.
- ISRC, label, catalog number, provider identifiers.

### 6.2 Effective Values

```text
EffectiveYear = ProviderYear ?? PersonalYear
EffectiveGenre = ProviderGenre ?? PersonalGenre
EffectiveBpm = TrustedProviderBpm ?? LocalBpm
EffectiveKey = TrustedProviderKey ?? LocalKey
```

Key output uses Camelot notation.

### 6.3 Display And Filename

```text
Artist tag: Artist 1, Artist 2
Title tag: Base Title (Remixer Remix)
Filename: Artist 1, Artist 2 - Base Title (Remixer Remix).ext
```

Do not duplicate an existing remix suffix. Preserve the original extension and sanitize only characters invalid for the destination filesystem.

### 6.4 PersonalGenre

Allowed initial values:

- `Day Instrumental`
- `Day Vocal`
- `Night Instrumental`
- `Night Vocal`
- `TechnoHouse`
- `Tribal`
- `Remember`

It is mandatory for ordinary catalog tracks. Persist it in PostgreSQL and in the audio file. Initial MP3 mapping is `TXXX:PERSONAL_GENRE`; define and test an explicit mapping for each supported format.

Classification evaluates energy/context, vocal presence, and tribal/ethnic character separately. `Tribal` and `TechnoHouse` are dominant categories. For pending tracks, show the best proposal, confidence, evidence, and strongest alternative; user approval is mandatory.

## 7. Root Policies

| Capability | Main | Pending | Remember | Sessions |
|---|---:|---:|---:|---:|
| Index automatically | Yes | Yes | Yes | Yes |
| Browse/play | Yes | Yes | Yes | Separate section |
| Analyze automatically on detection | No | No | No | Never |
| Start provider analysis manually | Selected tracks | New/selected tracks | Selected tracks | Never |
| Require approval before writing analyzed data | Always | Ambiguous match, artwork, PersonalGenre | Configured review flow | N/A |
| Manual metadata editing | Yes | Yes | Yes | Yes |
| Automatic move | Never | After approval and explicit confirmation | Never | Never |
| PersonalGenre rule | From folder/manual | Inferred and approved | Always Remember | Not required |

Pending analysis states:

- New.
- Queued.
- Analyzing.
- Awaiting match review.
- Awaiting artwork review.
- Awaiting PersonalGenre approval.
- Approved, awaiting apply/move.
- Unidentified.
- Postponed/rejected.
- Failed.
- Completed/moved.

Analysis identity is based on content hash and analysis version. A changed file becomes new. A previously analyzed unchanged file remains visibly analyzed and is excluded from the default run unless explicitly included.

## 8. Provider And Analysis Strategy

### 8.1 Matching Pipeline

1. Read current tags and technical properties with no mutation.
2. Calculate a content hash and duration.
3. Prefer a valid existing ISRC.
4. Generate Chromaprint fingerprint and query AcoustID.
5. Resolve recordings/releases through MusicBrainz.
6. Search/enrich through Discogs.
7. Score candidates using ISRC, fingerprint, artists, normalized title/mix, and duration.
8. Return the selected high-confidence candidate or a ranked review list.

Respect provider identity requirements, attribution, licensing, caching rules, and rate limits. Implement retry with bounded exponential backoff for transient failures, but do not retry validation/authentication failures indefinitely.

### 8.2 Local Audio Analysis

Use containerized FFmpeg/ffprobe and a proven analysis library/tool to obtain:

- Duration and codec properties.
- BPM fallback.
- Musical key fallback, converted to Camelot.
- Waveform peak data.
- Evidence useful for PersonalGenre suggestions where technically feasible.

Record tool and algorithm versions so results can be invalidated and recalculated after an upgrade.

### 8.3 Beatport

Define a provider interface that can support Beatport later, but do not implement automated scraping. Authorized API integration and manual assisted URL import remain future work.

## 9. Artwork Contract

Artwork is always reviewed before replacement.

When applying artwork:

1. Prefer approved provider artwork.
2. Otherwise select one existing embedded image.
3. Correct orientation.
4. Convert to JPEG.
5. Fit within 500x500 while preserving aspect ratio.
6. Never upscale.
7. Remove all embedded images.
8. Embed exactly the approved normalized image.
9. Verify the resulting file can be reopened and contains one image.

Store the previous artwork in reversible history until expiry.

## 10. Filesystem And Mutation Contract

- Resolve every requested path under its configured canonical root.
- Reject symlink/path traversal escapes.
- Check available write permission and destination collision before mutation.
- Write tags to a temporary sibling file, reopen and verify it, then atomically replace the original where the filesystem supports it.
- A move from Pending combines verified tag write, final filename, and destination move as a recoverable operation.
- Approval does not authorize movement. Show exact source, destination, final filename, and collisions, then ask separately.
- Destination uses `<PersonalYear>/House - <PersonalGenre>/`.
- `PersonalYear` is the personal/download year, normally current year for new pending tracks.
- If a destination filename exists, block and open duplicate/manual resolution. Never append `(1)`.
- External moves in catalog roots update the index and create a metadata proposal when folder-derived values change; they do not write automatically.

History retains full prior tags, artwork, filename, and path for 365 days. A scheduled job removes expired history and blobs. Permanent deletion cannot be undone and requires reinforced confirmation for both individual and bulk operations.

## 11. Explorer / Player Requirements

### 11.1 Explorer

- Folder tree for Main, Pending, and Remember.
- Dense desktop data grid.
- Full-text search.
- Filters for artist, title, effective year, provider genre, and PersonalGenre.
- Combined filters, sorting, and multi-selection.
- Actions for play, queue, manual edit, bulk edit, reanalyze, playlist, duplicate review, and delete.
- Search results can replace or append to the queue.

### 11.2 Global Player

- One playback engine shared by global and contextual controls.
- Queue from manual selection, folder, search, or playlist.
- Add, remove, reorder, clear, previous, next, repeat, volume, and seek.
- Shuffle creates a non-repeating cycle and preserves history for Previous.
- Show artwork, artists, title, effective genre, PersonalGenre, BPM, and Camelot key.
- Display an interactive waveform using server-generated cached peaks.
- Persist queue, current track, shuffle state, and position; restore paused.
- Resolve moved tracks by stable ID and skip missing/deleted entries.
- Stream audio with HTTP Range support.

### 11.3 Keyboard And Notifications

- Context-aware shortcuts for play/pause, previous/next, queue, save, approve/reject, and pending navigation.
- Ignore shortcuts while the user types.
- Never bypass destructive confirmations.
- Persist job-completion/error notifications and link to their results.

## 12. Playlists

Support:

- Manual playlists with stable ordered track references.
- Smart playlists that store filter rules and update dynamically.
- Initial useful rules such as Day, Day Vocals, Night, Tribal, and TechnoHouse across all years.
- Add/replace queue and shuffle playback.

Export UTF-8 M3U8 using configurable profiles. Translate `/music/...` container paths to host-visible UNC/Windows paths for AIMP and Traktor. Offer relative and absolute modes when valid, and preview unresolved paths before export.

## 13. Duplicates

Detect:

- Exact duplicates by content hash.
- Possible duplicates by fingerprint, ISRC, normalized metadata, and duration.

Comparison shows and allows playback of both files, with path, format, bitrate, sample rate, size, duration, metadata, and artwork. Recommend which file to retain using technical quality and exact version evidence. Never delete automatically.

## 14. Sessions

Sessions have their own routes, queries, explorer, search, and player. Do not reuse the global track queue.

Requirements:

- Browse by year/folder.
- Read actual duration from audio.
- Highlight title, year, artwork, and duration while allowing other supported manual metadata edits.
- Never call music metadata providers or automatic track analysis.
- Show an interactive waveform.
- Do not persist playback position.
- Resolve tracklist in this order:
  1. `<audio-base-name>.txt` beside the audio.
  2. `tracklist.txt` beside the audio when the directory represents one session.
  3. Mark ambiguous if multiple audios share one generic tracklist.
- Display tracklist text read-only and preserve the source file unchanged.

## 15. Delivery Phases

### 15.1 Development And Test Deployment Workflow

Git and the canonical GitHub repository, <https://github.com/dgomezc/dj-tracks-and-sessions>, are the source of truth. The normal delivery path is:

1. Clone and keep the working tree in the WSL Linux filesystem on the Windows 11 development PC, not under `/mnt/c`, unless a documented tool constraint requires otherwise. This preserves Linux/Docker filesystem semantics and avoids cross-filesystem performance penalties.
2. Develop on a feature branch in WSL. Run the local API in `Development` with its connection string in .NET User Secrets. Run repeatable local builds, tests, and Docker Compose verification against disposable fixture roots and an externally supplied non-versioned `ConnectionStrings__Postgres` value because containers cannot access User Secrets. This local verification is the delivery gate. Never mount the production music library in the local loop.
3. After the local build/test gate passes, optionally build immutable `linux/amd64` application images locally from the exact Git commit with an explicit commit-derived tag as an architecture/image gate. Do not transfer those images to the NAS or use a floating `latest` tag. Push the feature branch and commits to GitHub manually.
4. When a usable development/test version exists, manually clone or pull the repository on the NAS, checkout the selected branch, tag, or commit, prepare an ignored NAS-local environment/secret file and disposable roots, and validate the Compose configuration. No SSH connection to the NAS, WSL-to-NAS transfer, image archive, registry, or deployment script is used.
5. From the selected NAS checkout, manually build/run the migration target against the external development PostgreSQL instance, run `docker compose up -d`, and perform health and smoke checks.

The manual operator must stop when configuration, migration, startup, health, or smoke verification fails. Credentials and secrets must not appear in source control, documented command arguments, or deployment logs.

Rollback selects the previous immutable image tag. Restore an operator-created pre-migration database backup only when the prior application version is incompatible with the migrated schema; never attempt an implicit schema downgrade. The deployment documentation must state the migration compatibility boundary and make migration and rollback failures visible.

GitHub Actions automation and publishing images to GHCR may be evaluated later. Neither is part of the current plan or delivery gate.

### Phase 0: Feasibility Spikes

Goal: remove technical uncertainty before building product workflows.

Work units:

1. Read/write round trip for representative MP3, FLAC, M4A, AIFF, and WAV fixtures.
2. Custom PersonalGenre mapping per format.
3. Artwork normalization and one-image verification.
4. FFmpeg/ffprobe, Chromaprint, BPM/key analysis in `linux/amd64` container.
5. HTTP Range streaming and waveform peak generation.
6. Provider clients proving MusicBrainz, Discogs, and AcoustID requests, limits, and normalized fixtures.

Exit criteria:

- A written capability matrix identifies safe read/write fields per format.
- Sample files survive verified mutation without losing unknown tags.
- Tool images run on the target NAS architecture.
- Provider adapters have deterministic fixture tests.
- Any unsupported format behavior is explicitly degraded instead of guessed.

### Phase 1: Application Foundation

Goal: boot a production-shaped empty application.

Work units:

1. Solution, projects, dependency direction, and test projects.
2. API Problem Details and FluentResults mapping.
3. FluentValidation registration and explicit async validation pipeline.
4. Code First PostgreSQL schema, reviewed EF Core migrations through Npgsql, and health checks.
5. Dockerfiles and Docker Compose with four music mounts, an external PostgreSQL connection, and an application-data persistent volume.
6. Blazor Blueprint shell, Spanish UI, light/dark themes, and separate main navigation areas.
7. Durable job and notification primitives.
8. Repeatable local WSL commands or scripts for build, test, Compose verification, and immutable `linux/amd64` image builds from an exact Git commit.
9. Manual NAS test deployment documentation covering clone-on-NAS version selection, NAS-local configuration, disposable roots, Compose validation, explicit external-database migration, startup, health checks, smoke checks, rollback, and cleanup.

Exit criteria:

- Compose starts API and Web on `linux/amd64`.
- Health checks verify database, configured mounts, and required tools.
- Frontend communicates only through API contracts.
- Persistence integration tests run against disposable PostgreSQL containers; Compose verification uses an externally supplied non-versioned connection string and disposable filesystem roots.
- Local verification fails before image build or deployment when required builds, tests, or Compose checks fail; locally built images use an explicit immutable commit-derived tag.
- A manually selected usable version can be tested on the NAS using NAS-only configuration and disposable roots without hardcoded credentials.
- A failed configuration, migration, startup, health check, or smoke check is visible without enabling real-library mounts.

### Phase 2: Library Index (Foundation Complete Through Work Unit 4)

Goal: establish a trustworthy, read-only catalog.

Completed work units:

1. Configure and validate root policies.
2. Scan supported audio and session TXT files.
3. Extract technical properties and current tags.
4. Calculate stable hash identity incrementally.

Deferred original work units:

5. Reconcile renamed, moved, changed, and missing files. Superseded for immediate delivery by the simpler reconciliation in Phase 2A Work Unit 1.
6. Watch roots and schedule full reconciliation. Deferred; Phase 2A uses explicit manual scans.
7. Build folder tree, catalog grid, search, and required filters. Superseded for immediate delivery by Phase 2A Work Unit 2's smaller read-only catalog.

Original Phase 2 exit criteria, now deferred or fulfilled through Phase 2A and later roadmap work:

- All four roots index without modifying files.
- Repeated scans are idempotent.
- Moves are correlated by identity where possible.
- Traversal outside configured roots is rejected.
- Main/Pending/Remember are visibly distinct; Sessions is separate.

Phase 2 is not the immediate implementation queue. Continue with Phase 2A below; return to deferred Phase 2 work only through an explicit plan amendment.

### Phase 2A: Catalog-First Delivery (Immediate)

Goal: deliver a useful catalog and the smallest safe organization workflow before automation, provider analysis, playback, and specialist features.

This stage is synchronous and manually triggered. It persists only the minimal catalog model, keeps Sessions separate, and preserves all existing filesystem safety boundaries.

Work units:

1. **Persist an explicit read-only scan (complete in `b1f4df6`).** Scan the four configured roots through the existing confined scanner, extraction, and incremental hashing components; synchronously upsert minimal catalog records, report per-file failures, reconcile unambiguous same-root hash matches, and mark missing records without deleting anything.
2. **Deliver the read-only Track Management screen (next).** Add ordinary catalog query endpoints and one Spanish desktop screen for manual scan, root-separated Main/Pending/Remember browsing, basic text search, simple filters, track detail, and loading/empty/failure states. Sessions is excluded. No playback, provider actions, bulk actions, metadata writes, or automatic refresh.
3. **Close Track Management with safe one-MP3 editing.** Add the same screen's exact before/after preview and explicit submit for supported MP3 text fields, including `TXXX:PERSONAL_GENRE`; use confined paths, sibling temporary writes, reopen verification, atomic replacement, and clear unsupported-format failures.
4. **Close Track Management with one approved Pending-to-Main move.** Add the same screen's separate confirmation of exact source, destination, final filename, and collision result after an approved edit; block collisions and update the catalog only after a verified move.

Exit criteria:

- An explicit scan persists a disposable four-root fixture without source-byte changes, remains idempotent, and reports failures independently.
- The Track Management screen browses Main, Pending, and Remember through ordinary catalog API contracts; Sessions never enters its route, queries, filters, detail, or future queue affordances.
- A single MP3 text edit preserves unknown tags and leaves the source unchanged on any validation, verification, confinement, or replacement failure.
- A Pending MP3 move requires separate exact confirmation, blocks collisions without suffixes, and never moves Main or Remember files.

Rollback boundary: each unit rolls back its migration, API/Web changes, and disposable-fixture tests; source media remains untouched except for the explicitly confirmed, verified MP3 write or Pending move.

The original Phase 2 Work Units 5-7 and all screens after Track Management remain later roadmap scope. Their automation and feature requirements are not prerequisites for Phase 2A and must not be described as the next immediate work.

### Screen-First Delivery Sequence

1. **Track Management screen.** Complete Phase 2A Work Units 2-4: read-only scan/browse/detail UI, one-MP3 safe edit, and one separately confirmed Pending move. Do not start Player or Sessions UI before its exit criteria pass.
2. **Player screen.** Complete the catalog Player UI and its supporting Range/API, shared playback service, queue, visible states, and tests. It provides one shell-mounted Player surface, not a contextual mini-player. Sessions remains outside the player and its queue. Waveforms, persisted playback, shuffle/repeat, mini-player integration, and shortcuts are deferred unless explicitly added to this screen's approved work units.
3. **Sessions screen.** Complete isolated Sessions queries, explorer/detail, its own player, read-only tracklist resolution, visible ambiguity/error states, and tests. Sessions must never enter track search, the global queue, playlists, duplicate detection, providers, or automatic analysis.
4. **Remaining screens.** Only after the first three screens close, sequence Analyzer/Tagger, playlists, duplicates, advanced metadata/history/deletion, and operational screens as independent vertical work units.

### Phase 3: Player Screen

Goal: deliver the complete catalog Player screen after Track Management closes.

Work units:

1. Range-enabled audio endpoint with format-appropriate content type.
2. One shared frontend playback service and shell-mounted Player surface; no contextual mini-player.
3. In-memory queue controls: add, remove, clear, previous, next, volume, and seek.
4. Player loading, unavailable, ended, and single-active-audio states with focused tests.

Exit criteria:

- Seeking works without downloading the full file first.
- Navigation does not interrupt playback.
- The shell-mounted Player remains the only catalog playback control surface in this phase.
- Sessions never enters the global queue or Player screen.

Deferred after the Player screen: persisted queue/position, shuffle/repeat, waveform, contextual mini-player integration, and keyboard shortcuts. When a mini-player is approved, it must coordinate through the same frontend playback service.

### Phase 4: Sessions Screen

Goal: deliver the complete isolated Sessions screen after Player closes.

Work units:

1. Session queries and year/folder explorer.
2. Session detail and supported manual metadata editing.
3. Separate player with no persisted position.
4. Tracklist resolution, ambiguity state, and read-only display.

Exit criteria:

- Sessions do not appear in track searches, global queue, playlists, or duplicate jobs.
- No automatic provider analysis is available for Sessions.
- Matching TXT displays while the session plays and remains unchanged.

### Phase 5: Advanced Safe Metadata Editing

Goal: edit original files with preview, verification, and undo.

Work units:

1. Format capability API and metadata editor.
2. Canonical title/artist/remix normalization.
3. Filename preview, sanitization, and collision validation.
4. Individual atomic tag write and post-write verification.
5. Bulk patch semantics and per-file outcomes.
6. PersonalGenre embedded-tag mappings.
7. Artwork review and normalization.
8. Reversible history and 365-day cleanup.
9. Individual and bulk permanent deletion confirmation.

Exit criteria:

- No write occurs without an exact before/after preview where review is required.
- Unsupported fields are disclosed and not silently dropped.
- Undo restores tags, artwork, name, and path for verified fixtures.
- Failed batch items do not invalidate successful independent items.

### Phase 6: Identification And Analysis

Goal: generate explainable metadata proposals.

Work units:

1. Chromaprint generation and AcoustID lookup.
2. MusicBrainz search/resolution.
3. Discogs enrichment.
4. Normalized provider model and provenance.
5. Candidate scoring and confidence thresholds.
6. Ranked match-review UI with contextual playback.
7. Local BPM and key fallback.
8. PersonalGenre classifier with evidence and alternative.

Exit criteria:

- Ambiguous matches never mutate files.
- Every proposed field exposes source and confidence.
- Main-library reanalysis always creates an approval proposal.
- Remember proposals force PersonalGenre to Remember.
- Key writes as valid Camelot notation.

### Phase 7: Advanced Pending Workflow

Goal: process the inbox safely from detection to confirmed movement.

Work units:

1. Pending states, notifications, and default selection of unprocessed files.
2. Manual start for all new or selected files.
3. Review workspace combining candidates, artwork, PersonalGenre, and mini-player.
4. Approval without movement authorization.
5. Destination calculation from PersonalYear and approved PersonalGenre.
6. Movement preview and explicit individual/bulk confirmation.
7. Collision/duplicate handoff and recoverable apply operation.

Exit criteria:

- Detection never starts analysis.
- Previously analyzed unchanged files are not reprocessed by default.
- No file moves before explicit destination confirmation.
- A collision blocks the move without creating a suffixed filename.
- Completed tracks appear correctly in Main after reconciliation.

### Phase 8: Playlists And Duplicates

Goal: support listening workflows and safe library cleanup.

Work units:

1. Manual playlists.
2. Smart playlist rule model and initial presets.
3. Queue integration.
4. M3U8 export and path profiles for AIMP/Traktor.
5. Exact duplicate groups.
6. Possible duplicate scoring, comparison, and retention recommendation.

Exit criteria:

- Smart playlists update when metadata changes.
- Manual playlist order survives file moves.
- Export preview contains host-visible paths.
- Duplicate deletion remains an explicit confirmed action.

### Phase 9: NAS Hardening And Release

Goal: prove safe operation against a representative copy before mounting the real library.

Work units:

1. Resource limits, cancellation, retries, and graceful shutdown.
2. Structured logs and diagnostics bundle without secrets.
3. Database backup/restore instructions and migration recovery.
4. NAS permission and path-mapping validation.
5. Representative-library acceptance run.
6. Deployment, upgrade, rollback, and disaster-recovery documentation.
7. Production deployment rehearsal using exact immutable tags, pre-migration backup, migration compatibility notes, and previous-tag rollback.

Exit criteria:

- Full workflow passes against a disposable representative library.
- Restart during analysis/tagging has a documented safe outcome.
- Real-library mount is not enabled until the user accepts the dry run.
- Operational documentation explains that application history is not a NAS backup.
- Backup restore is proven from a disposable database, and rollback documentation distinguishes image rollback from database restore.
- Production deployment configuration keeps NAS secrets and host paths outside Git and records the deployed immutable image tags.

## 16. Verification Strategy

### Unit Tests

- Metadata formatting and fallbacks.
- Camelot conversion.
- PersonalGenre precedence and evidence.
- Match scoring and confidence decisions.
- Smart playlist rules.
- Destination and filename calculation.
- Root policy decisions.

### Integration Tests

- API contracts and Problem Details.
- PostgreSQL migrations, constraints, transactions, and optimistic concurrency through EF Core/Npgsql.
- Root canonicalization and path traversal rejection.
- Scan/reconciliation behavior.
- Tag write/read round trips for every supported format.
- Artwork replacement and history restoration.
- Range responses and waveform cache.
- Pending apply/move and collision rollback.
- M3U8 path translation.
- Session tracklist association.

### Runtime Acceptance

- Use a disposable fixture library that mirrors all four roots.
- Include malformed tags, missing artwork, multiple artwork frames, ambiguous remixes, duplicate files, collisions, moved files, and long sessions.
- Never use the production music mount for automated tests.

## 17. MVP Completion Checklist

- [ ] Docker Compose runs reliably on the NAS.
- [ ] All roots are indexed and governed by their policy.
- [ ] Explorer/search/filter/player workflows work from a PC browser.
- [ ] Original tags can be edited atomically and undone for one year.
- [ ] MusicBrainz, Discogs, AcoustID, BPM, key, and waveform workflows operate through durable jobs.
- [ ] Main reanalysis is approval-only.
- [ ] Pending processing requires manual start, PersonalGenre/artwork review, and movement confirmation.
- [ ] Remember remains in place with PersonalGenre Remember.
- [ ] Smart/manual playlists and M3U8 exports work for AIMP/Traktor paths.
- [ ] Duplicates are identified and compared without automatic deletion.
- [ ] Sessions remain independent and display their TXT tracklists.
- [ ] Spanish UI, light/dark themes, keyboard shortcuts, and persistent notifications are complete.
- [ ] Representative-library dry run is approved before production use.

## 18. Future Backlog

- Authelia-protected remote access.
- Evaluate OpenSubsonic-compatible access for external Windows and Android applications. OpenSubsonic is the open evolution of the legacy Subsonic API and provides an interoperability target for desktop and mobile clients. At implementation time, compare the currently maintained OpenSubsonic-compatible servers and an adapter exposed by this application; do not preselect a server now. Decide whether to deploy the selected server beside this application or expose a compatible API only after proving representative Windows and Android client compatibility for catalog tracks and Sessions, streaming with HTTP Range or transcoding as applicable, artwork and metadata, and playlists where product rules permit them. Preserve Sessions as a separate core collection even if a compatibility boundary presents them to clients. Document authentication, authorization, transport security, rate limiting, and exposure risks before any use beyond the LAN.
- Authorized Beatport provider.
- Manual assisted Beatport URL import.
- Session track timestamps and waveform markers.
- Click-to-seek tracklist entries.
- Tracklist editing.
- Traktor NML export.
- PersonalGenre suggestion tuning from approved decisions.
