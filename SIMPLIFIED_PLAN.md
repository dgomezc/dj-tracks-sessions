# Simplified Delivery Proposal (Adopted as Phase 2A)

This proposal has been adopted into `PLAN.md` as **Phase 2A: Catalog-First Delivery**. `PLAN.md` is now the executable source of truth; this file is retained as historical rationale and detail for the decision. It does not change settled product decisions by itself. Its purpose is to deliver a safe, useful single-user NAS application for approximately 800 tracks before investing in automation, durable workflows, and specialist listening features.

The MVP has two outcomes, in order:

1. A trustworthy, read-only catalog that can be scanned on demand and browsed in the desktop web UI.
2. Safe manual metadata editing and an explicitly confirmed move from Pending to Main.

The non-negotiable boundaries remain unchanged: PostgreSQL and Docker on the NAS; four confined roots; no automatic file mutation or deletion; no automatic analysis; separate Sessions; and an exact source, destination, and filename confirmation before a Pending move.

## Current Baseline

The repository is clean on `develop` and the test suite passed locally on 2026-08-22: 30 integration tests and 4 unit tests.

| Area | Implemented evidence | Status |
|---|---|---|
| Solution foundation | API, Web, Domain, Infrastructure, Contracts, and unit/integration test projects exist. | Complete enough for MVP work |
| API foundations | FluentResults-to-Problem Details mapping, explicit async FluentValidation helper, OpenAPI/Scalar, health endpoint, and a configuration diagnostic endpoint exist. | Complete enough for MVP work |
| PostgreSQL foundation | EF Core/Npgsql `ApplicationDbContext`, an initial reviewed migration, health probing, and Testcontainers-oriented test infrastructure exist. | Complete enough for MVP work |
| Docker/NAS foundation | API and Web images, Compose configuration, four mounts, external PostgreSQL configuration, local gate, and manual NAS deployment documentation exist. | Complete enough for MVP work |
| Web foundation | Blazor shell, Spanish UI, persistent theme support, separate API client boundary, and Razor code-behind convention exist. | Complete enough for MVP work |
| Root safety | Four canonical, non-overlapping roots; root policies; existing-path confinement; traversal and symlink-escape rejection are implemented and tested. | Complete |
| Read-only discovery | Recursive ordinary-root discovery and shallow Session folder discovery are implemented. Scanner tests prove no tag reads or source mutation. | Complete for an initial scan |
| Read-only extraction | TagLibSharp2 reads technical properties and common tags with per-file errors; source files remain unchanged. | Complete for catalog intake |
| Stable identity | Incremental, root-confined SHA-256 hashing is implemented with per-file failures and unchanged-source tests. | Complete for catalog intake |
| Durable primitives | `Job`, `JobNotification`, and their tables exist. No worker, queue, or user workflow uses them. | Present but not an MVP dependency |
| Catalog persistence and UI | No catalog entity/table, reconciliation record, scan API, catalog endpoint, scan command, or catalog grid is implemented. | Not started |
| File mutation | No approved metadata write, rename, move, collision validation, history, or deletion workflow is implemented. | Not started |

Recent commits substantiate the active implementation boundary: `369df45` root policies, `92a2113` confined scanning, `ad0bb0d` read-only metadata extraction, and `cdeb32d` incremental content identity. The active original plan position is Phase 2, after Work Unit 4; Phase 0 is not fully exited because its remaining tool, provider, artwork, and mapping spikes were never needed for read-only catalog intake.

## Delivery Decision

Use explicit, user-triggered scans and direct synchronous processing for the approximately 800-track MVP. A full scan, extraction, hash, and database upsert is bounded and observable at this scale. It does not need a durable job subsystem, filesystem watcher, notification center, or provider pipeline before it produces value.

The API remains the only boundary allowed to access PostgreSQL and the filesystem. The UI remains Spanish and desktop-first. Every write remains an explicit request with a preview and verification; the simplified path removes automation, not safety.

## Scope Decisions

### Keep Now

| Item | Rationale |
|---|---|
| Docker Compose with external PostgreSQL and four configured roots | Required deployment model and a settled NAS boundary. |
| EF Core Code First migrations and PostgreSQL integration tests | Required persistence choice; a real database protects migration and query behavior. |
| API/Web separation, Domain/Infrastructure boundary, Problem Details, and async validation | Existing foundations are small and make filesystem failures safe and comprehensible. |
| Canonical root confinement, symlink escape rejection, non-overlapping roots, and content hashes | Safety-critical. Hashes make later rename reconciliation possible without treating paths as identity. |
| Explicit scan command, read-only extraction, and per-file error reporting | Delivers a trustworthy catalog while protecting original media. |
| Separate Session records and routes | Sessions must never leak into tracks, the global queue, playlists, duplication, or analysis. |
| Manual metadata preview, atomic sibling write, reopen verification, and collision blocking | The minimum safe mutation contract. |
| Pending move preview and a separate exact confirmation | Required product safety rule; approval to edit never implies approval to move. |

### Defer

| Item | Why it waits |
|---|---|
| Filesystem watchers and scheduled reconciliation | An on-demand scan is sufficient for 800 tracks. Add periodic reconciliation only after real NAS usage demonstrates a need. |
| Durable jobs, restart recovery, progress notifications, and background worker | The first scans and single-file writes can run synchronously. Introduce durable jobs only for operations that demonstrably exceed acceptable request time. |
| Provider matching, Chromaprint, AcoustID, MusicBrainz, Discogs, BPM/key, and genre classification | They add external rate limits, evidence models, tool images, confidence logic, and review UIs before the manual catalog workflow is proven. Manual tagging remains usable without them. |
| Artwork normalization and embedded artwork replacement | It is destructive metadata work with format-specific risk. Deliver after normal text metadata writes have proven safe on the real supported fixture set. |
| Playback, Range streaming, queue persistence, waveform generation, mini-player, and keyboard shortcuts | They are a separate product capability and do not unblock cataloging or safe organization. Start only after the catalog is usable. |
| Playlists, exports, duplicate detection, smart rules, and deletion UI | They depend on a stable catalog and metadata model but do not create the initial catalog or safe pending workflow. |
| Session player, waveform, and manual Session editing | Preserve session indexing and isolation now; defer richer Session behavior until the track catalog and safe mutation path are established. |
| Bulk editing and reversible history | Start with one-file operations. Add batch outcomes and undo only after one-file atomic write semantics have been verified end to end. |
| FFmpeg target-container proof and provider capability spikes | Required only when analysis, waveforms, or provider features are approved for delivery. |

### Remove Or Simplify

| Original direction | Simplified decision | Rationale |
|---|---|---|
| Nine sequential phases and a 14-item MVP checklist | Replace with four outcome-based work units below. | The current plan sequences every future feature before usable catalog value. |
| Rich initial persisted model: provenance per field, artwork, analysis runs, candidates, proposals, history, playlists, playback session, duplicate groups, jobs, and notifications | Persist only root-scoped catalog records, technical/tag snapshot, content hash, scan timestamps, and minimal Session records. | Most records have no producer or user workflow yet. Create them when their feature is approved. |
| Automatic watcher plus periodic reconciliation | Manual scan first; later add a simple scheduled scan only if necessary. | Watcher correctness on NAS mounts is operational complexity without an MVP need. |
| Durable job system as a prerequisite for scans and hashes | Call existing read-only components from an explicit scan command; retain existing primitives without extending them. | A single user's bounded library does not require queue recovery to build the first catalog. |
| Full-text search engine and advanced combined filters initially | Use indexed PostgreSQL queries with simple text contains, root, year, and PersonalGenre filters. | PostgreSQL is already required and is sufficient at 800 tracks. |
| Generalized capability API and all-format mutation parity | Support MP3 text metadata first; present all other formats as read-only until their exact field-preservation behavior is proven. | The documented capability spike proves unknown-tag preservation only for MP3 and FLAC, while M4A, AIFF, and WAV are explicitly degraded. |
| Bulk patching, 365-day reversible history, permanent deletion, and cleanup jobs | Exclude deletion from this MVP; defer batch and undo after one-file writes. | These features multiply failure modes and do not help create or safely organize the first catalog. No automatic deletion remains prohibited. |
| Full analyzer state machine | Use a simple Pending item state: indexed, edited, ready to move, moved, or failed. | Analysis is deferred, so its queued/review/rejected states would be unused. |
| Waveform as a playback requirement | No waveform in this MVP. | It adds FFmpeg tooling, cache lifecycle, API, and UI complexity unrelated to safe catalog organization. |

## Minimal Persisted Model

Create only the records needed to display an indexed library and safely target a later manual operation.

| Record | Required fields |
|---|---|
| `CatalogTrack` | Database ID, root type, root-relative path, extension, content hash, byte length, last observed UTC timestamp, missing flag, and concurrency token |
| `CatalogTrackMetadata` | Current title, artists, album, year, genre, BPM, key, duration, bitrate/sample rate, and editable personal fields when supplied |
| `SessionCatalogItem` | Database ID, root-relative folder/audio path, tracklist paths, optional artwork paths, duration, last observed UTC timestamp, validity/issues |

Do not create placeholders for providers, analysis, artwork history, queues, playlists, duplicate groups, or notifications. The current `Job` and `JobNotification` tables may remain untouched; they are not part of the simplified delivery path.

## MVP Delivery Plan

Each work unit is dependency-ordered and should ship with focused behavior tests, an integration test against PostgreSQL where persistence is involved, a disposable-root runtime scenario, and a documented rollback boundary.

### Work Unit 1: Persist an explicit read-only scan

Build a scan command for the four configured roots. It calls the existing confined scanner, extraction coordinator, and incremental hasher, then upserts the minimal catalog model in PostgreSQL.

- Scan is manually started from the API; it never runs because of a filesystem event.
- Each ordinary audio file produces an independent success or error outcome.
- Hash matches update a renamed path within the same root when unambiguous; otherwise retain records and mark missing rather than deleting them.
- Sessions are persisted separately and never written to the ordinary catalog.
- No source file is modified.

Acceptance criteria:

- A disposable four-root fixture scans into PostgreSQL without source-byte changes.
- Traversal and symlink escapes remain rejected.
- Repeating an unchanged scan is idempotent.
- An ordinary file renamed within its root keeps its database identity through its unchanged hash.
- A missing file is marked missing; no database record or filesystem file is automatically deleted.
- Session audio and TXT data cannot appear in ordinary catalog queries.

### Work Unit 2: Deliver the read-only catalog

Add catalog and Sessions query endpoints, then a dense Spanish desktop grid with root selection, basic text search, and simple filters. Display extraction failures visibly rather than hiding files.

- Ordinary catalog filters: root, text, year, and PersonalGenre when present.
- Sessions use their own route and list, with folder, audio, tracklist availability, artwork availability, duration, and issues.
- Do not include playback, provider actions, bulk actions, advanced full-text infrastructure, or automatic refresh.

Acceptance criteria:

- A user can manually scan, browse Main/Pending/Remember distinctly, search known tags, and inspect a track's technical and current-tag snapshot.
- A user can browse Sessions separately and see missing audio or tracklist issues.
- The Web project accesses data exclusively through API contracts.
- No UI action mutates media.

### Work Unit 3: Safely edit one MP3's text metadata

Deliver a single-track manual editor for explicitly selected MP3 files. It supports the fields whose MP3 mapping is proven by the capability work: artists, normalized title/remix display, year, genre, and `PersonalGenre` as `TXXX:PERSONAL_GENRE`.

- Show an exact before/after tag and filename preview.
- Resolve the selected path under its configured root immediately before writing.
- Write to a sibling temporary file, reopen it, verify supported values and preserved unknown MP3 tags, then atomically replace the source.
- Block unsupported formats with a stable `tag.unsupported` result; do not fall back to partial silent writes.
- Re-scan or directly refresh the affected catalog record only after verified success.
- Remember always writes `PersonalGenre=Remember`; Main and Pending require an explicit personal genre.

Acceptance criteria:

- A selected MP3 write has a visible before/after preview and requires explicit submit.
- The source is unchanged if validation, confinement, temp write, reopen verification, or replacement fails.
- An unknown MP3 tag survives the verified write fixture.
- An M4A, AIFF, WAV, FLAC, or AAC request receives a clear unsupported/deferred response and makes no changes.
- A failed write does not corrupt or remove the catalog record.

### Work Unit 4: Move one approved Pending MP3 into Main

Add a distinct move-confirmation flow after a successful manual edit. It calculates the final Main destination from approved `PersonalYear`, `PersonalGenre`, and canonical filename rules.

- Show exact source, destination directory, final filename, and collision result.
- Require a separate confirmation; saving tags or editing metadata does not authorize the move.
- Reject existing destination names without suffix generation.
- Verify the tagged temporary artifact before replacing/moving; update the catalog only after a completed move.
- Main and Remember never expose an automatic or direct move operation through this workflow.

Acceptance criteria:

- A Pending MP3 cannot move until the user confirms the exact calculated destination and filename.
- A collision blocks the operation without changing either file.
- A verified successful move appears in Main after refresh and the Pending record no longer points to a nonexistent file.
- Cancel, validation failure, root escape, and replacement/move failure leave source media intact and report a stable error.
- No move is triggered by scanning, watching, tagging another file, or provider analysis.

## Migration And Compatibility

The proposed catalog migration is additive. It adds catalog tables and relationships but does not alter the existing `application_settings`, `jobs`, or `job_notifications` schema. Existing development databases migrate forward through a reviewed EF Core migration; no `EnsureCreated`, implicit recreation, or database downgrade is allowed.

The current read-only scanner, extractor, and hasher can be reused without compatibility wrappers. Persist hashes as the existing uppercase SHA-256 hexadecimal value. Existing code has no persisted catalog records, public catalog API, or mutation behavior to preserve, so no data backfill or client compatibility layer is required beyond the first explicit scan.

Do not promise cross-format write compatibility. The existing documented spike proves unknown-tag preservation only for MP3 and FLAC fixtures. This proposal intentionally limits writes to MP3 until each added format obtains its own exact preservation and atomic-write proof. A future format extension is an additive capability, not an implicit behavior change.

The existing job tables and classes may remain in the migration history but must not be expanded or made operational for these work units. They can be removed only in a later, explicitly approved migration after verifying that no deployed database or operational procedure relies on them.

## Out of Scope Until Re-approved

- Automatic or scheduled file mutation, deletion, analysis, provider matching, or movement.
- Filesystem watcher-driven work.
- Non-MP3 writes, artwork modification, bulk edits, undo/history, and deletion.
- Playback, queues, waveforms, keyboard shortcuts, playlists, exports, and duplicate workflows.
- Provider integrations, BPM/key detection, classification, and external tool images.
- Any change to the separate-collection rules for Sessions.

## Historical Immediate Work Unit At Proposal Time

At proposal time, the next work unit was **Persist an explicit read-only scan**. It is now the first work unit of the adopted Phase 2A in `PLAN.md`.

It is the smallest missing vertical slice that turns the already implemented root confinement, discovery, extraction, and stable hash work into a usable database-backed catalog. It has no media mutation rollback risk: rollback is the new migration, catalog persistence code, endpoint, and disposable-fixture tests; source media remains untouched.
