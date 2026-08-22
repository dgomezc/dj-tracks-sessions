# Phase 2A Work Unit 1: Explicit Read-Only Scan (Retired)

## Scope

Retired by the filesystem-first Track Management decision. The historical implementation remains in migration history so already-applied databases are upgradeable, but no runtime scan endpoint or persisted ordinary-track catalog remains.

The former `POST /library/scan` implementation was removed. Direct Track Management browsing reads configured roots and current file tags without a scan prerequisite.

Only the separate `SessionCatalogItem` persistence remains from this historical unit. Ordinary track rows and snapshots are removed by the follow-up migration.

## Verification

Focused commands:

```bash
dotnet build DjTracksSessions.slnx
dotnet test DjTracksSessions.slnx --no-restore
git diff --check
```

Migration generation used the temporary tool installation and a placeholder design-time connection string:

```bash
dotnet tool install dotnet-ef --tool-path /tmp/opencode/dotnet-ef
ConnectionStrings__Postgres='Host=localhost;Database=placeholder;Username=placeholder;Password=placeholder' \
  /tmp/opencode/dotnet-ef/dotnet-ef migrations add PersistExplicitReadOnlyCatalogScan \
  --project src/DjTrackSessions.Infrastructure \
  --startup-project src/DjTrackSessions.Infrastructure \
  --output-dir Migrations
```

The build passed with 0 warnings and 0 errors. The full existing test suite passed: 4 unit tests and 30 integration tests. PostgreSQL/Testcontainers runtime verification was not executed because Docker is unavailable in this environment. The catalog model test verifies table names and the separate metadata relationship without substituting SQLite or EF InMemory.

## Runtime And Safety Evidence

The historical scan tests used disposable roots only. They are retired because no runtime feature consumes the persisted scan pipeline. Current Track Management tests use disposable roots and prove behavior through direct filesystem adapters without constructing or querying `ApplicationDbContext`.

## Migration And Rollback

Migration history: `20260822094000_PersistExplicitReadOnlyCatalogScan` remains unchanged. `20260822121845_DropPersistedCatalogTables` removes only `catalog_track_metadata` and `catalog_tracks`; settings, jobs, notifications, and `session_catalog_items` remain.

Rollback boundary: the historical scan implementation is not restored. If an operator must downgrade an already migrated deployment, restore the pre-migration database backup and use the migration's reviewed `Down` operation. No source media rollback is needed because the retired work unit performs no writes or deletes.
