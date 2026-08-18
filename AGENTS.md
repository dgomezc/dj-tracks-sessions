# Agent Instructions

These rules are mandatory for every human or AI contributor.

## Start Here

1. Read `README.md`, `PLAN.md`, and `DECISIONS.md` before proposing or implementing changes.
2. Identify the active phase and work unit in `PLAN.md`.
3. Inspect the existing code before choosing an implementation.
4. Do not reinterpret a product rule. Record a genuine ambiguity and ask one focused question.

## Project Skills And MCPs

- Load `.opencode/skills/execute-plan-work-unit/SKILL.md` for every implementation or correction of a `PLAN.md` work unit.
- Load `.opencode/skills/dotnet-vertical-slice/SKILL.md` when changing API slices, contracts, FluentResults, or FluentValidation behavior.
- Load `.opencode/skills/audio-filesystem-safety/SKILL.md` before touching scanning, tags, artwork, rename, move, delete, or reconciliation behavior.
- Load `.opencode/skills/frontend-design/SKILL.md` before designing or visually refactoring Blazor UI, layouts, themes, or UI states.
- Use the Blazor Blueprint MCP for exact component APIs, setup, patterns, and version changes. Do not guess component parameters.
- Use Playwright MCP only when a running UI needs interactive inspection or manual-flow verification. It does not replace automated Playwright .NET tests.
- Keep MCP queries narrow and task-specific to avoid unnecessary context.

## Product Boundaries

- The product is a single-user web application running only in Docker on a Linux x86-64 NAS.
- The MVP is local-network only and has no authentication or authorization.
- Keep the ASP.NET Core API separate from the Blazor frontend. The frontend must not access the database, filesystem, metadata providers, or audio tools directly.
- Sessions are a separate collection. Do not include them in the track catalog, global player queue, playlists, duplicate detection, or automatic analysis.
- Never move files from the main library or Remember automatically.
- Never analyze or modify cataloged files automatically merely because a filesystem event occurred.
- Never delete a file automatically. Permanent deletion always requires explicit confirmation.
- Never overwrite a low-confidence or ambiguous provider match.
- Never move a pending track without showing and confirming its exact source, destination, and final filename.
- Do not add Beatport scraping as an automatic provider. Beatport integration requires authorized API access; manual assisted import is a future item.

## Architecture

- `DjTracksSessions.Api` owns vertical-slice `Features`; each slice contains its endpoint, queries, commands, validations, mappers, handlers, and behavior tests, not global technical folders such as `Controllers`, `Services`, or `Repositories`.
- Slices use `DjTrackSessions.Domain` for entities and invariants and `DjTrackSessions.Infrastructure` adapters; do not move persistence or external implementations into the API.
- `DjTrackSessions.Domain` owns framework-independent entities, value objects, and domain invariants; it must not reference EF Core.
- `DjTrackSessions.Infrastructure` owns the EF Core Code First `DbContext`, Fluent `IEntityTypeConfiguration<T>` mappings, migrations, and filesystem, database, provider, image, and audio-tool adapters.
- Use Entity Framework Core Code First with the Npgsql provider for all application persistence. PostgreSQL is the only supported production database.
- Create schema changes through reviewed EF Core migrations. Do not use `EnsureCreated` for application startup or production deployment.
- Return expected failures through `FluentResults`. Do not use exceptions for validation, missing matches, collisions, or other expected outcomes.
- Use `FluentValidation` explicitly and asynchronously. Do not rely on ASP.NET synchronous auto-validation.
- Map API failures to one consistent Problem Details contract with stable machine-readable error codes.
- Keep API contracts independent from persistence entities and Blazor view models.
- Use UTC for stored timestamps and explicit local dates for personal/release years.

## Filesystem Safety

- Treat configured mount roots as security boundaries. Reject paths that escape their root after canonicalization.
- Identify audio files by persistent database ID plus content hash/fingerprint, never by path alone.
- Write tags atomically through a temporary file in the same filesystem and replace only after verification.
- Preserve unsupported and unknown tags unless a format-specific rule explicitly removes them.
- Keep exactly one embedded artwork image after an approved artwork operation.
- Detect destination collisions before renaming or moving. Never invent numeric filename suffixes.
- A batch operation must report success or failure per file and must not hide partial completion.
- Filesystem watchers are hints. Reconcile every root periodically because NAS events may be lost.

## Metadata Invariants

- Format artists as `Artist 1, Artist 2`.
- Format remixes as `Title (Remixer Remix)` in both title tag and filename.
- Format filenames as `Artist 1, Artist 2 - Title (Remixer Remix).ext`.
- Store provider, personal, and effective values separately.
- `EffectiveYear = ProviderYear ?? PersonalYear`.
- `EffectiveGenre = ProviderGenre ?? PersonalGenre`.
- `PersonalGenre` is mandatory for ordinary tracks.
- Remember tracks always use `PersonalGenre=Remember`.
- MP3 stores the personal genre as `TXXX:PERSONAL_GENRE`; define explicit equivalent mappings per supported format.
- Write musical key in Camelot notation.
- Provider BPM/key take precedence over local analysis when their provenance is trusted.

## UI Rules

- User-facing text is Spanish. Code, identifiers, comments, API contracts, and technical documentation remain English.
- Razor files under `src/DjTracksSessions.Web` contain markup and directives only; keep all C# code in the adjacent `.razor.cs` partial class code-behind file.
- Optimize for a desktop browser. Mobile-specific design is out of scope.
- Preserve a clear boundary between `Explorer / Player` and `Analyzer / Tagger`.
- Use Blazor Blueprint components and its light/dark theming before building custom primitives.
- Keep destructive actions visually distinct and require confirmation.
- Keyboard shortcuts must not trigger while typing in an input and must not bypass confirmations.
- Only one audio element may play at a time; global and contextual players coordinate through one frontend playback service.

## Testing And Delivery

- Tests ship with the behavior they verify.
- Prefer behavior-focused unit tests for pure rules and integration tests for API, PostgreSQL, filesystem, tag writing, and external adapter contracts.
- Run persistence integration tests against disposable PostgreSQL containers. Do not substitute EF Core InMemory or SQLite for PostgreSQL behavior.
- Every filesystem test uses an isolated temporary root and disposable sample files.
- Never run destructive tests against a mounted real music library.
- Provider tests use recorded fixtures or test doubles by default; live tests must be explicitly opted in.
- Each work unit must document its focused test command, runtime verification, and rollback boundary.
- Keep commits as reviewable behavioral units and use Conventional Commits without AI attribution.
- Do not mark a phase complete until all acceptance criteria in `PLAN.md` pass.

## Plan Maintenance

- Update `PLAN.md` when scope or ordering changes.
- Update `DECISIONS.md` when a product or architecture decision changes.
- Do not silently add compatibility layers, alternate hosts, multi-user behavior, cloud deployment, or future features.
