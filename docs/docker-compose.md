# Docker Compose

The Compose stack deploys the API and Web only. It does not deploy, start, mount, or own PostgreSQL containers or database volumes.

Required variables:

- `ConnectionStrings__Postgres`
- `MAIN_LIBRARY_PATH`
- `PENDING_LIBRARY_PATH`
- `REMEMBER_LIBRARY_PATH`
- `SESSIONS_LIBRARY_PATH`

Optional variables:

- `ASPNETCORE_ENVIRONMENT` defaults to `Production`
- `API_BASE_URL` defaults to `http://api:8080` for the Web container
- `API_PORT` defaults to `8080`
- `WEB_PORT` defaults to `8081`

The API receives `ConnectionStrings__Postgres` unchanged from an ignored local or NAS-local environment/secret file. Do not derive or split it into credentials in Compose. .NET User Secrets are for local API development only and are not available inside Compose containers; future production uses a separately provisioned database.

The local WSL gate is build/test-only. Do not treat local Compose execution as NAS deployment or transfer anything from WSL to the NAS. The exact local command set is documented in [local-gate.md](local-gate.md).

Run Compose manually on the NAS from a manually selected repository checkout only when a usable development/test version exists. Follow [deployment/nas-manual.md](deployment/nas-manual.md). The NAS uses the base Compose file and the external NAS PostgreSQL development instance; this repository does not deploy a PostgreSQL container or volume.
