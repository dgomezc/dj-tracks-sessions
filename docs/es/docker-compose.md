# Docker Compose

> Traducción al español. El documento original en inglés se conserva en `../../docs/docker-compose.md`.

El stack de Compose de la Fase 1 se configura mediante variables de entorno y no guarda secretos en el repositorio.

Variables obligatorias:

- `POSTGRES_PASSWORD`
- `MAIN_LIBRARY_PATH`
- `PENDING_LIBRARY_PATH`
- `REMEMBER_LIBRARY_PATH`
- `SESSIONS_LIBRARY_PATH`

Variables opcionales:

- `POSTGRES_DB` usa `djtracksessions` por defecto.
- `POSTGRES_USER` usa `djtracksessions` por defecto.
- `ASPNETCORE_ENVIRONMENT` usa `Production` por defecto.
- `API_PORT` usa `8080` por defecto.
- `WEB_PORT` usa `8081` por defecto.

La API usa `ConnectionStrings__Postgres` para conectarse al servicio PostgreSQL en la red interna de Compose.

Para el flujo de verificación local en WSL, usa `scripts/wsl/compose-verify.sh` con raíces fixture desechables. El conjunto exacto de comandos está documentado en [local-gate.md](local-gate.md).

Para el flujo de despliegue de prueba en el NAS, usa [deployment/nas-test.md](deployment/nas-test.md) y la sobreescritura versionada `docker-compose.nas-test.yml`.
