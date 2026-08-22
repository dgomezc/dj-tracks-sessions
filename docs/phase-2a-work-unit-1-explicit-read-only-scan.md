# Phase 2A Work Unit 1: Explicit Read-Only Scan (Complete)

## Scope

Completed in `b1f4df6` (`feat: persist explicit catalog scans`). The next work unit is Phase 2A Work Unit 2, the read-only Track Management screen. This record preserves the completed unit's scope and evidence.

The API exposes `POST /library/scan`. The request is manually triggered and runs the existing confined scanner, read-only metadata extraction, and incremental SHA-256 hashing during the HTTP request. It does not create jobs, watchers, provider records, analysis records, or file mutations.

Ordinary roots persist `CatalogTrack` and its `CatalogTrackMetadata` snapshot. Sessions persist `SessionCatalogItem` records in a separate table and are never added to ordinary catalog records. Successful files are upserted by root and relative path; an unambiguous same-root unchanged hash also preserves the existing database identity after a rename. Files not observed by a successful scan are marked missing, never deleted. Each extraction or hash failure is returned independently in `failures`.

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

The scan route is `POST /library/scan`. Runtime fixture scenarios remain confined to disposable roots: source bytes are only read, repeated scans are idempotent, failures are independent, same-root hash renames retain identity, missing files are marked, and Sessions remain separate. No real configured music root was accessed.

## Migration And Rollback

Migration: `20260822094000_PersistExplicitReadOnlyCatalogScan` (`PersistExplicitReadOnlyCatalogScan`). It adds `catalog_tracks`, `catalog_track_metadata`, and `session_catalog_items` only; the existing application settings and durable job tables are unchanged.

Rollback boundary: revert the API/domain/infrastructure changes, remove the scan endpoint and migration, and restore the pre-migration database backup only if an operator needs to downgrade an already migrated deployment. No source media rollback is needed because this work unit performs no writes or deletes.
