# Docker Compose

> Traducción al español. El documento original en inglés se conserva en `../../docs/docker-compose.md`.

El stack de Compose despliega únicamente API y Web. No despliega, inicia, monta ni gestiona contenedores o volúmenes PostgreSQL.

Variables obligatorias:

- `ConnectionStrings__Postgres`
- `MAIN_LIBRARY_PATH`
- `PENDING_LIBRARY_PATH`
- `REMEMBER_LIBRARY_PATH`
- `SESSIONS_LIBRARY_PATH`

Variables opcionales:

- `ASPNETCORE_ENVIRONMENT` usa `Production` por defecto.
- `API_BASE_URL` usa `http://api:8080` por defecto para el contenedor Web.
- `API_PORT` usa `8080` por defecto.
- `WEB_PORT` usa `8081` por defecto.

La API recibe `ConnectionStrings__Postgres` sin cambios desde un archivo de entorno o secretos local o del NAS e ignorado por Git. No derives ni separes credenciales en Compose. .NET User Secrets se usa solo para el desarrollo local de la API y no está disponible dentro de los contenedores de Compose; producción usará una base aprovisionada por separado.

La puerta local de WSL se limita a compilación y pruebas. No trates la ejecución local de Compose como un despliegue en el NAS ni transfieras nada desde WSL al NAS. El conjunto exacto de comandos locales está documentado en [local-gate.md](local-gate.md).

Ejecuta Compose manualmente en el NAS desde un checkout del repositorio seleccionado manualmente solo cuando exista una versión utilizable de desarrollo/pruebas. Sigue [deployment/nas-manual.md](deployment/nas-manual.md). El NAS usa el Compose base y la instancia PostgreSQL externa de desarrollo; este repositorio no despliega un contenedor ni un volumen PostgreSQL.
