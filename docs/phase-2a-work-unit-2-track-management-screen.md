# Phase 2A Work Unit 2: Read-Only Track Management Screen

## Outcome

Deliver the first complete portion of the Spanish desktop Track Management screen on top of the persisted explicit scan from `b1f4df6`. It closes the read-only scan/browse experience before any player or Sessions screen begins.

## Scope

- Add ordinary catalog query contracts and API slices for Main, Pending, and Remember only.
- Add a manual scan action and dense desktop grid with root selection, basic text search, year and PersonalGenre filters, track detail, and clear loading, empty, failure, and unavailable states.
- Surface scan and extraction failures without hiding successfully indexed tracks.
- Keep Web access behind API contracts only.

## Acceptance Criteria

- A user can trigger a manual scan, browse each ordinary root distinctly, search known metadata, and inspect its technical/current-tag snapshot from the Spanish Track Management screen.
- Sessions records never appear in the route, query response, grid, filter values, detail view, or any future queue affordance on this screen.
- No control writes media, moves files, starts provider analysis, queues audio, or refreshes automatically.
- Focused API/UI tests cover root separation and durable Spanish navigation/action labels.

## Explicit Limits

- This work unit does not add playback, queueing, metadata editing, Pending moves, bulk actions, provider actions, watchers, scheduled reconciliation, or automatic refresh.
- It does not create a Sessions page. Sessions starts only after the Track Management screen is closed and the Player screen is complete.

## Verification And Rollback

Record the focused test command, exact result, disposable-root runtime scenario, and rollback boundary when the work unit is implemented. Do not access a configured or production music root. Rollback is limited to this unit's query/UI changes and tests; `b1f4df6` scan persistence remains intact.
