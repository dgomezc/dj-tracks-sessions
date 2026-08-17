# Manual NAS Development Deployment

This manual is for testing a usable development/test version on the Linux x86-64 NAS. The operator works on the NAS itself. No SSH connection to the NAS is used by this project, and no WSL-to-NAS transfer, `scp`, image archive, registry, or automated deployment script is part of the workflow.

## Quick Path

From a NAS shell:

```bash
git clone <repository-url> <checkout-directory>
cd <checkout-directory>
git checkout <selected-branch-or-commit>
mkdir -p <nas-config-directory> <nas-test-root>/main <nas-test-root>/pending <nas-test-root>/remember <nas-test-root>/sessions <nas-test-root>/app-data
chmod 700 <nas-config-directory>
touch <nas-config-directory>/djtracksessions.env
chmod 600 <nas-config-directory>/djtracksessions.env
docker compose --env-file <nas-config-directory>/djtracksessions.env config
docker build --target migrations -t djtracksessions-api-migrations:<selected-version> -f src/DjTracksSessions.Api/Dockerfile .
docker run --rm --env-file <nas-config-directory>/djtracksessions.env djtracksessions-api-migrations:<selected-version>
docker compose --env-file <nas-config-directory>/djtracksessions.env up -d
```

Replace every angle-bracket placeholder. Do not paste real credentials into this document, shell history, Git, or a command argument.

## Prerequisites

- A usable development/test version has been selected; this is not a production release procedure.
- The NAS has Git, Docker Engine, Docker Compose v2, and `curl`.
- The NAS has access to the external development PostgreSQL instance.
- The NAS has writable disposable roots for Main, Pending, Remember, Sessions, and application data.
- The real music library is not mounted.

## Version And Checkout

Clone the canonical repository on the NAS, or pull it if the checkout already exists. Inspect the selected branch, tag, or commit before starting. Record the selected commit for rollback and repeatability:

```bash
git fetch --tags --prune
git checkout <selected-branch-or-commit>
git rev-parse HEAD
```

The NAS Compose test happens only from this manually selected NAS checkout. Local WSL build/test commands remain a separate build gate; they do not transfer a deployment to the NAS.

## NAS-Local Configuration

Create an ignored file outside the checkout, or in a checkout-local ignored path, and fill it with placeholders replaced only on the NAS:

```dotenv
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__Postgres=<external-development-postgresql-connection-string>
MAIN_LIBRARY_PATH=<nas-test-root>/main
PENDING_LIBRARY_PATH=<nas-test-root>/pending
REMEMBER_LIBRARY_PATH=<nas-test-root>/remember
SESSIONS_LIBRARY_PATH=<nas-test-root>/sessions
API_PORT=8080
WEB_PORT=8081
```

Use a NAS-local secret/configuration mechanism with permissions restricted to the operator. Do not commit the file or include real values in logs, tickets, or documentation. The PostgreSQL server, database, user, and password are external to this repository.

## Disposable Roots

Create empty or representative test roots and verify that they are not paths in the real music library. Never let a missing variable cause Compose to bind an unintended directory. Keep application data disposable unless the test explicitly requires retaining it.

## Validate And Migrate

Run configuration validation before starting services:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env config
```

Build and run the migration target from the selected checkout. This applies reviewed EF Core migrations to the external development PostgreSQL instance; it does not create a PostgreSQL container or volume:

```bash
docker build --target migrations -t djtracksessions-api-migrations:<selected-version> -f src/DjTracksSessions.Api/Dockerfile .
docker run --rm --env-file <nas-config-directory>/djtracksessions.env djtracksessions-api-migrations:<selected-version>
```

If the migration fails, stop. Do not start the stack until the failure is understood and the database operator has handled any required recovery.

## Start And Verify

Start the application manually from the selected checkout:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env up -d
docker compose --env-file <nas-config-directory>/djtracksessions.env ps
curl -fsS http://localhost:8080/health
curl -fsS http://localhost:8081/
```

Check logs and status when a check fails:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env logs --tail=200 api web
docker compose --env-file <nas-config-directory>/djtracksessions.env ps
```

## Rollback

Stop the stack, checkout the previous known-good branch, tag, or commit, revalidate configuration, and repeat the migration compatibility check before starting it again:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env down
git checkout <previous-known-good-branch-or-commit>
docker compose --env-file <nas-config-directory>/djtracksessions.env config
docker compose --env-file <nas-config-directory>/djtracksessions.env up -d
```

Checking out an earlier application version does not automatically roll back the database schema. Never attempt an implicit schema downgrade. For an incompatible migration, use only a database backup created and validated by the database operator before the migration.

## Cleanup

After testing, stop and remove the test stack and disposable application volume if it is no longer needed:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env down -v --remove-orphans
rm -rf <nas-test-root>
```

Keep or remove the NAS-local configuration according to the NAS secret-management policy. Never remove a real library path or real database data as part of this test cleanup.
