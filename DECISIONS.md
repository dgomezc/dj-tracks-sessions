# Product Decisions

This file records settled decisions. Change them only with explicit product approval.

## Deployment

- Product name: **DJ Tracks & Sessions**.
- Web-only application deployed with Docker Compose on a Linux x86-64 NAS.
- Single user, local network, no authentication in the MVP.
- Future external access may be protected by Authelia at the reverse-proxy boundary.
- Approximately 800 tracks with slow monthly growth; PostgreSQL and one worker are sufficient.
- Git and the canonical GitHub repository, <https://github.com/dgomezc/dj-tracks-and-sessions>, are the development source of truth.
- Development and debugging run on a Windows 11 PC through WSL. The working tree normally lives in the WSL Linux filesystem rather than `/mnt/c` for Linux/Docker behavior and performance.
- The local feature-branch loop runs builds, tests, and Docker Compose against disposable fixture roots and PostgreSQL; it never mounts the production music library.
- Repeatable local WSL build, test, and Docker Compose verification is the delivery gate. After it passes, immutable `linux/amd64` images are built locally from the exact Git commit with an explicit commit-derived tag; a floating `latest` tag is never used.
- The current LAN NAS test target is `192.168.68.100`, but host, SSH user, deployment path, and image tag remain configurable and credentials are never committed.
- NAS test deployment uses a versioned Compose definition, NAS-only secrets/configuration, and disposable or representative test roots before any real-library mount.
- One documented parameterized WSL operation transfers the exact tagged images directly to the NAS over SSH, loads them without requiring a registry, backs up PostgreSQL, runs explicit migrations, starts the versioned Compose test stack, and performs health/smoke checks with visible failure.
- Rollback selects the previous immutable image tag. The database is restored from the pre-migration backup only when migration compatibility requires it; schema downgrade is never implicit.
- Feature branches and commits are pushed to GitHub manually. GitHub Actions automation and GHCR publication may be evaluated later but are not part of the current plan.

## Technology

- .NET 10, ASP.NET Core API, and Blazor Interactive Server.
- Blazor Blueprint UI with persistent light and dark themes.
- API and frontend are separate projects and runtime boundaries.
- Vertical Slice Architecture with FluentResults and FluentValidation.
- Entity Framework Core with the Npgsql provider is the persistence API; PostgreSQL is the only production database.
- Interface in Spanish, optimized for desktop use.

## Files And Collections

- Docker Compose configures the main, pending, Remember, and Sessions roots.
- The main library is already cataloged and is reanalyzed only on explicit request. Every proposed change requires approval.
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
- PersonalGenre is always stored separately in the database and audio file.
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
- Contextual mini-player shares the same playback engine.
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
