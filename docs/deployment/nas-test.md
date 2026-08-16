# NAS Test Deployment

The Phase 1 NAS test deployment keeps the workflow local to WSL until later phases.

## Compose

Use `docker-compose.nas-test.yml` as the versioned NAS override. It pins the API and Web services to the exact immutable image tag that was transferred to the NAS and remaps the data mounts to NAS-specific paths.

## Deployment Command

Use `scripts/wsl/deploy-nas-test.sh` with parameters:

- `NAS_HOST`
- `NAS_USER`
- `NAS_DEPLOY_DIR`
- `IMAGE_TAG`
- `NAS_MAIN_LIBRARY_PATH`
- `NAS_PENDING_LIBRARY_PATH`
- `NAS_REMEMBER_LIBRARY_PATH`
- `NAS_SESSIONS_LIBRARY_PATH`
- `NAS_APP_DATA_PATH`
- `NAS_POSTGRES_DATA_PATH`
- `API_IMAGE_NAME` defaults to `djtracksessions-api`
- `WEB_IMAGE_NAME` defaults to `djtracksessions-web`
- `MIGRATIONS_IMAGE_NAME` defaults to `djtracksessions-api-migrations`
- `POSTGRES_DB` defaults to `djtracksessions`
- `POSTGRES_USER` defaults to `djtracksessions`
- `POSTGRES_PASSWORD`
- `BASE_COMPOSE_FILE` defaults to `docker-compose.yml`
- `REMOTE_COMPOSE_FILE` defaults to `docker-compose.nas-test.yml`

The script:

1. Saves the exact tagged API, Web, and migration images as archives.
2. Transfers the base compose file, the NAS compose override, and the archives to the NAS over SSH.
3. Loads the images on the NAS without a registry.
4. Starts PostgreSQL on the NAS test stack.
5. Captures a pre-migration PostgreSQL backup.
6. Runs explicit EF Core migrations with the transferred exact tag.
7. Starts the versioned Compose stack.
8. Verifies API and Web health checks.
9. Performs a smoke check against the Web root page.

## Rollback Boundary

Rollback uses the previous immutable image tag. Restore the pre-migration PostgreSQL backup only if the migration is not forward-compatible with the previous application version.

## Notes

- Do not use a floating `latest` tag.
- Do not point the NAS test stack at the real music library.
- Credentials and secrets remain outside Git and must be provided through the environment.
