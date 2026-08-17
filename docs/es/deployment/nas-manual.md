# Despliegue manual de desarrollo en el NAS

Este manual sirve para probar una versión utilizable de desarrollo/pruebas en el NAS Linux x86-64. El operador trabaja en el propio NAS. El proyecto no usa una conexión SSH al NAS ni transferencia WSL-NAS, `scp`, archivos de imágenes, registry o scripts de despliegue automatizado.

## Ruta rápida

Desde una shell del NAS:

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

Sustituye todos los marcadores entre ángulos. No pegues credenciales reales en este documento, el historial de shell, Git ni argumentos de comandos.

## Requisitos previos

- Se ha seleccionado una versión utilizable de desarrollo/pruebas; no es un procedimiento de producción.
- El NAS tiene Git, Docker Engine, Docker Compose v2 y `curl`.
- El NAS puede acceder a la instancia externa de PostgreSQL de desarrollo.
- El NAS tiene raíces escribibles y desechables para Main, Pending, Remember, Sessions y los datos de la aplicación.
- No se monta la biblioteca musical real.

## Versión y checkout

Clona el repositorio canónico en el NAS o actualízalo si ya existe. Inspecciona la rama, etiqueta o commit seleccionado antes de iniciar. Registra el commit para poder repetir o revertir la prueba:

```bash
git fetch --tags --prune
git checkout <selected-branch-or-commit>
git rev-parse HEAD
```

La prueba de Compose en el NAS se ejecuta únicamente desde este checkout seleccionado manualmente en el NAS. Los comandos de compilación y pruebas locales de WSL siguen siendo una puerta de compilación separada; no transfieren un despliegue al NAS.

## Configuración local del NAS

Crea un archivo ignorado fuera del checkout o en una ruta ignorada del checkout y rellénalo sustituyendo los marcadores solo en el NAS:

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

Usa un mecanismo de secretos/configuración local del NAS con permisos restringidos al operador. No subas el archivo al repositorio ni incluyas valores reales en logs, incidencias o documentación. El servidor, la base, el usuario y la contraseña de PostgreSQL son externos a este repositorio.

## Raíces desechables

Crea raíces de prueba vacías o representativas y verifica que no sean rutas de la biblioteca musical real. Nunca permitas que una variable ausente haga que Compose monte un directorio no previsto. Mantén desechables los datos de aplicación salvo que la prueba requiera conservarlos.

## Validar y migrar

Valida la configuración antes de iniciar los servicios:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env config
```

Construye y ejecuta el destino de migraciones desde el checkout seleccionado. Aplica las migraciones EF Core revisadas a la instancia externa de PostgreSQL de desarrollo; no crea un contenedor ni un volumen PostgreSQL:

```bash
docker build --target migrations -t djtracksessions-api-migrations:<selected-version> -f src/DjTracksSessions.Api/Dockerfile .
docker run --rm --env-file <nas-config-directory>/djtracksessions.env djtracksessions-api-migrations:<selected-version>
```

Si falla la migración, detente. No inicies el stack hasta entender el fallo y que el administrador de la base haya gestionado la recuperación necesaria.

## Iniciar y verificar

Inicia la aplicación manualmente desde el checkout seleccionado:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env up -d
docker compose --env-file <nas-config-directory>/djtracksessions.env ps
curl -fsS http://localhost:8080/health
curl -fsS http://localhost:8081/
```

Consulta logs y estado si falla una comprobación:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env logs --tail=200 api web
docker compose --env-file <nas-config-directory>/djtracksessions.env ps
```

## Rollback

Detén el stack, cambia al branch, tag o commit anterior conocido como bueno, vuelve a validar la configuración y comprueba la compatibilidad de la migración antes de iniciarlo de nuevo:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env down
git checkout <previous-known-good-branch-or-commit>
docker compose --env-file <nas-config-directory>/djtracksessions.env config
docker compose --env-file <nas-config-directory>/djtracksessions.env up -d
```

Cambiar a una versión anterior no revierte automáticamente el esquema de la base. Nunca intentes degradarlo implícitamente. Si una migración es incompatible, usa únicamente una copia de seguridad creada y validada por el administrador antes de la migración.

## Limpieza

Al terminar, detén y elimina el stack de pruebas y el volumen desechable de la aplicación si ya no hace falta:

```bash
docker compose --env-file <nas-config-directory>/djtracksessions.env down -v --remove-orphans
rm -rf <nas-test-root>
```

Conserva o elimina la configuración local del NAS según la política de secretos del NAS. Nunca elimines una ruta de biblioteca real ni datos reales de la base como parte de esta limpieza.
