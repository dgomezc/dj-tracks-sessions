# Despliegue de prueba en el NAS

> Traducción al español. El documento original en inglés se conserva en `../../../docs/deployment/nas-test.md`.

El despliegue de prueba en el NAS de la Fase 1 mantiene el flujo local en WSL hasta fases posteriores.

## Compose

Usa `docker-compose.nas-test.yml` como sobreescritura versionada del NAS. Fija los servicios API y Web a la etiqueta de imagen inmutable exacta que se transfirió al NAS y reasigna los montajes de datos a rutas específicas del NAS.

## Comando de despliegue

Usa `scripts/wsl/deploy-nas-test.sh` con estos parámetros:

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
- `API_IMAGE_NAME` usa `djtracksessions-api` por defecto.
- `WEB_IMAGE_NAME` usa `djtracksessions-web` por defecto.
- `MIGRATIONS_IMAGE_NAME` usa `djtracksessions-api-migrations` por defecto.
- `POSTGRES_DB` usa `djtracksessions` por defecto.
- `POSTGRES_USER` usa `djtracksessions` por defecto.
- `POSTGRES_PASSWORD`
- `BASE_COMPOSE_FILE` usa `docker-compose.yml` por defecto.
- `REMOTE_COMPOSE_FILE` usa `docker-compose.nas-test.yml` por defecto.

El script:

1. Guarda las imágenes exactas etiquetadas de API, Web y migraciones como archivos de imagen.
2. Transfiere al NAS por SSH el archivo Compose base, la sobreescritura Compose del NAS y los archivos de imagen.
3. Carga las imágenes en el NAS sin un registry.
4. Inicia PostgreSQL en el stack de prueba del NAS.
5. Captura una copia de seguridad de PostgreSQL previa a la migración.
6. Ejecuta migraciones explícitas de EF Core con la etiqueta exacta transferida.
7. Inicia el stack de Compose versionado.
8. Verifica las comprobaciones de salud de API y Web.
9. Ejecuta una comprobación smoke contra la página raíz de Web.

## Límite de rollback

El rollback usa la etiqueta de imagen inmutable anterior. Restaura la copia de seguridad de PostgreSQL previa a la migración únicamente si la migración no es compatible hacia delante con la versión anterior de la aplicación.

## Notas

- No uses una etiqueta flotante `latest`.
- No apuntes el stack de prueba del NAS a la biblioteca musical real.
- Las credenciales y los secretos permanecen fuera de Git y deben proporcionarse mediante el entorno.
