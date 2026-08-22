# Phase 2A Work Units 2-4: Track Management Screen

## Outcome

Deliver the first complete Spanish desktop Track Management screen with direct browsing of the configured Main, Pending, and Remember files, including read-only file/tag browsing, one safe MP3 edit, and one separately confirmed Pending-to-Main move. No persisted catalog scan or track ID is involved.

## Scope

- Add filesystem-backed API contracts and slices for Main, Pending, and Remember only. Resolve every requested path under its configured canonical root and expose relative paths only.
- Add a dense desktop explorer with root selection, direct-child folder tree, selected-folder audio list, file-tag detail, and clear loading, empty, failure, and unavailable states. Read current technical properties and tags from each file; file tags are authoritative for display.
- Add exact MP3 before/after preview and explicit apply through a sibling temporary file, reopen verification, and atomic replacement.
- Add exact Pending source/destination/name/collision preview and independent movement confirmation.
- Surface per-file extraction failures without hiding successfully read files.
- Keep Web access behind API contracts only.
- Apply the Sonic Precision visual language: dense desktop data, tonal dark surfaces, ghost borders, system-font fallbacks, emerald primary actions, blue technical states, amber warnings, and explicit focus/error states while preserving the supported light theme.

## Design Reference Use

The `design/` directory is a visual-reference package. Implementers inspect its screenshots, `DESIGN.md`, and any local assets to extract hierarchy, palette, density, and state guidance. Generated Stitch HTML is not production source: do not copy its markup, CDN dependencies, remote images, font imports, future controls, or sample data. Rebuild only approved current behavior with semantic Blazor markup, local CSS, known Blazor Blueprint components, and system-font fallbacks.

Global navigation is a semantic desktop header limited to implemented routes: Inicio, Biblioteca, and Configuración. Main, Pending, and Remember remain contextual catalog-root controls, not global navigation; Player, Analyzer, Tagger, and Sessions remain absent until their approved screens exist.

## Acceptance Criteria

- A user can browse each ordinary root distinctly without a prior scan, navigate direct-child folders, and inspect current technical properties and tags from the Spanish Track Management screen.
- Sessions records never appear in the route, query response, grid, filter values, detail view, or any future queue affordance on this screen.
- No read-only control starts provider analysis, queues audio, or refreshes automatically.
- Unsupported formats fail with `tag.unsupported`; Remember edits require `PersonalGenre=Remember`.
- Symlink and traversal escapes are rejected; missing, unreadable, corrupt, and unsupported files produce visible per-file errors without hiding readable files.
- Pending movement blocks collisions and returns the verified filesystem state after the move.
- Focused safety/UI tests cover root separation, unsupported edits, Remember invariants, and durable Spanish navigation/action labels.

## Explicit Limits

- This screen does not add playback, queueing, bulk actions, provider actions, watchers, scheduled reconciliation, or automatic refresh.
- It does not create a Sessions page. Sessions starts only after the Track Management screen is closed and the Player screen is complete.

## Verification And Rollback

Verification recorded for the current implementation:

```text
dotnet build --no-restore: PASS (0 warnings, 0 errors)
dotnet test tests/DjTracksSessions.UnitTests/DjTracksSessions.UnitTests.csproj --no-restore: PASS after registering the filesystem library client and separating the page code-behind
dotnet test tests/DjTracksSessions.IntegrationTests/DjTracksSessions.IntegrationTests.csproj --no-restore: PASS (38 tests; fixture-only safety tests, no PostgreSQL boundary)
dotnet test --no-restore: PASS (5 unit tests, 38 integration tests)
git diff --check: PASS
```

Visual validation requires the API and Web processes to run separately with a disposable PostgreSQL database and four disposable roots; the API is not expected to start without those settings. No configured or production music root was accessed. Rollback is limited to the browse, metadata, move, Web, and fixture-test changes in this delivery.

The screen is not marked complete until a runtime check against disposable roots passes. WU2 filesystem browsing is implemented with direct API contracts and fixture coverage. WU3 and WU4 remain implemented from the prior partial work, but are not marked complete because their API/runtime evidence and the final visual check are still outstanding. No PostgreSQL access was required by the direct browsing tests; no PostgreSQL runtime was available or simulated.
