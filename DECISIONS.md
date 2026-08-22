# Product Decisions

This file records settled decisions. Change them only with explicit product approval.

## Delivery Sequencing

- **Approved Track Management browsing decision:** Track Management navigates the configured Main, Pending, and Remember files directly through confined filesystem access. PostgreSQL is not used for listing tracks, displaying tags, metadata editing, or Pending movement; tags read from each file are authoritative. Sessions remains separate.

- **Catalog persistence removal decision:** Track Management has no persisted track index. `CatalogTrack`, `CatalogTrackMetadata`, catalog scan persistence, and persisted track IDs are removed. Metadata edits and Pending moves accept an allowed root plus relative path, re-read current filesystem tags, and return filesystem state. PostgreSQL remains for persistence that has a real consumer or an explicitly planned future use.

- Adopt **screen-first delivery** after retiring the historical Phase 2A Work Unit 1 scan persistence. Finish the Track Management screen, including its browse, edit, and approved Pending-move flows, before starting the Player screen; finish Player before Sessions; start all remaining screens only afterwards.
- The Track Management MVP uses direct synchronous filesystem reads and explicit mutation confirmations. Filesystem watchers, scheduled reconciliation, catalog scan jobs, and background automation are deferred until a real consumer is approved.
- Persist only concepts with a current consumer or explicit approved future use. Track Management has no persisted ordinary-track catalog, metadata snapshot, content hash, scan observation, or persisted track ID; separate Sessions persistence remains independent.
- The immediate next work unit is Phase 2A Work Unit 2, the read-only portion of the Track Management screen. Provider analysis, playlists, duplicates, bulk operations, undo/history, deletion, and automation remain deferred. Player and Sessions are sequenced after the Track Management screen rather than being prerequisites for it.

## Deployment

- Product name: **DJ Tracks & Sessions**.
- Web-only application deployed with Docker Compose on a Linux x86-64 NAS.
- Single user, local network, no authentication in the MVP.
- Future external access may be protected by Authelia at the reverse-proxy boundary.
- Approximately 800 tracks with slow monthly growth; PostgreSQL and one worker are sufficient.
- Git and the canonical GitHub repository, <https://github.com/dgomezc/dj-tracks-and-sessions>, are the development source of truth.
- Development and debugging run on a Windows 11 PC through WSL. The working tree normally lives in the WSL Linux filesystem rather than `/mnt/c` for Linux/Docker behavior and performance.
- Local API development in the `Development` environment reads `ConnectionStrings:Postgres` from .NET User Secrets. Docker Compose verification uses an externally supplied non-versioned `ConnectionStrings__Postgres` value because User Secrets are not available inside containers; both flows never mount the production music library.
- Repeatable local WSL build and test commands, plus the optional exact-commit `linux/amd64` image-build check, are the delivery gate. Optional local Compose verification uses disposable roots only; it is not NAS deployment, and locally built images are never transferred.
- Docker Compose is tested on the NAS only when a usable development/test version has been manually cloned and selected on the NAS. No SSH connection, WSL-to-NAS transfer, image archive, registry, or automated deployment script is used.
- The manual NAS workflow uses NAS-only secrets/configuration, disposable or representative test roots before any real-library mount, explicit migration, startup, and health/smoke checks.
- The NAS PostgreSQL instance is the development database. The project does not deploy, start, mount, or own PostgreSQL containers or volumes. Docker/NAS connection strings stay in ignored external environment or secret files; a future production database is separate and must be provisioned before production deployment.
- Rollback selects the previous immutable image tag. Database migration rollback is an operational action owned by the database operator; schema downgrade is never implicit and restoration must use an operator-created pre-migration backup only when migration compatibility requires it.
- Feature branches and commits are pushed to GitHub manually. GitHub Actions automation and GHCR publication may be evaluated later but are not part of the current plan.

## Technology

- .NET 10, ASP.NET Core API, and Blazor Interactive Server.
- Blazor Blueprint UI with persistent light and dark themes.
- API and frontend are separate projects and runtime boundaries.
- `DjTracksSessions.Api` owns vertical-slice `Features`. Each feature contains its endpoint, queries, commands, validations, mappers, handlers, and behavior tests; it uses Domain invariants/entities and Infrastructure adapters without owning persistence or external implementations.
- Entity Framework Core with the Npgsql provider is the persistence API; PostgreSQL is the only production database.
- EF Core uses Code First. `DjTrackSessions.Domain` contains framework-independent entities, value objects, and invariants; `DjTrackSessions.Infrastructure` contains the DbContext, Fluent entity configurations, migrations, and external-service adapters.
- Interface in Spanish, optimized for desktop use.

## Files And Collections

- Docker Compose configures the main, pending, Remember, and Sessions roots.
- Main files are read directly from the configured root and are modified only through explicit approved operations.
- Pending files are detected automatically but analyzed only when requested.
- Approved pending files may be tagged, renamed, and moved only after movement confirmation.
- Remember tracks may be analyzed and edited but never moved; `PersonalGenre` is always `Remember`.
- Sessions are separate, manually edited, never automatically analyzed, and remain outside track playlists and the global player.
- Only audio files and session TXT tracklists are managed.

## Metadata

- Main artist output uses comma separation.
- Remix titles use `Title (Remixer Remix)`.
- Filename uses `Artist 1, Artist 2 - Title (Remixer Remix).ext`.
- Provider year wins in the Year tag; personal/download year is the fallback.
- Provider genre wins in the Genre tag; PersonalGenre is the fallback.
- PersonalGenre is stored in the audio file for the current filesystem-first Track Management flow; database persistence requires a separately approved consumer.
- Supported personal genres: Day Instrumental, Day Vocal, Night Instrumental, Night Vocal, TechnoHouse, Tribal, and Remember.
- Tribal and TechnoHouse take precedence over Day/Night and Vocal/Instrumental classification.
- Musical key is written in Camelot notation.
- MP3 is the first-class format; FLAC, M4A/AAC, AIFF, and WAV follow explicit capability rules.

## Artwork

- Artwork always requires user review.
- Each track has at most one embedded image.
- Provider artwork replaces embedded artwork when approved; otherwise existing artwork is normalized.
- Output is JPEG, maximum 500x500, preserving aspect ratio without upscaling.

## Providers And Analysis

- MVP providers: MusicBrainz, Discogs, and AcoustID/Chromaprint.
- BPM and key are analyzed locally when absent from trusted provider data.
- Beatport is optional and requires authorized API access.
- Low-confidence or ambiguous matches require manual selection.
- Pending PersonalGenre is inferred but always approved manually.

## Playback And Playlists

- Global persistent player with queue, shuffle, metadata, artwork, and waveform.
- When introduced, a contextual mini-player shares the same playback engine as the shell Player.
- Queue, current track, and position survive restarts and restore paused.
- Smart and manual playlists are supported.
- M3U8 exports support configurable NAS-to-Windows path mapping for AIMP and Traktor.
- Sessions use their own player and do not need playback-position persistence.

## Safety

- Original files are modified directly.
- No automatic backup copies; NAS backup is an operational responsibility.
- Reversible history is retained for 365 days, including prior artwork.
- Individual and bulk deletion are permanent, manual, and explicitly confirmed.
- Duplicate recommendations are advisory; deletion is never automatic.

## Future Scope

- Authelia and external access.
- OpenSubsonic, the open evolution of the legacy Subsonic API, is the future interoperability target for representative Windows and Android clients; this is an evaluation, not an MVP server selection. At implementation time, compare currently maintained compatible servers with an application-owned adapter and decide between side-by-side deployment and adapter exposure using compatibility and security evidence.
- The OpenSubsonic evaluation must prove tracks and Sessions, applicable Range streaming or transcoding, artwork/metadata, and playlists where core product rules allow them. Sessions remain a separate core collection regardless of compatibility presentation.
- Authorized Beatport API or manual assisted URL import.
- Session track markers and navigation by timestamp.
- Tracklist editing.
- Traktor NML export.
- Improving PersonalGenre suggestions from approved corrections.
