# Docker Compose

The Phase 1 Compose stack is configured from environment variables and does not commit secrets.

Required variables:

- `POSTGRES_PASSWORD`
- `MAIN_LIBRARY_PATH`
- `PENDING_LIBRARY_PATH`
- `REMEMBER_LIBRARY_PATH`
- `SESSIONS_LIBRARY_PATH`

Optional variables:

- `POSTGRES_DB` defaults to `djtracksessions`
- `POSTGRES_USER` defaults to `djtracksessions`
- `ASPNETCORE_ENVIRONMENT` defaults to `Production`
- `API_PORT` defaults to `8080`
- `WEB_PORT` defaults to `8081`

The API uses `ConnectionStrings__Postgres` to reach the PostgreSQL service on the internal Compose network.
