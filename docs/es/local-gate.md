# Puerta local de WSL

> Traducción al español. El documento original en inglés se conserva en `../../docs/local-gate.md`.

Ejecuta la verificación del desarrollo desde WSL con el árbol de trabajo en el sistema de archivos Linux.

## Comandos

```bash
dotnet user-secrets set "ConnectionStrings:Postgres" "<connection-string>" --project src/DjTracksSessions.Api
./scripts/wsl/build.sh
./scripts/wsl/test.sh
./scripts/wsl/compose-verify.sh
./scripts/wsl/build-images.sh
```

## Qué hace cada comando

- `build.sh` restaura y compila la solución en modo Release.
- `test.sh` ejecuta las pruebas unitarias y de integración en modo Release.
- `compose-verify.sh` es una comprobación opcional de contenedores locales contra raíces desechables y un valor externo no versionado de `ConnectionStrings__Postgres`. No es el flujo de despliegue del NAS.
- `build-images.sh` construye opcionalmente imágenes locales de API, Web y migraciones para `linux/amd64` desde el commit Git actual exacto. Estas imágenes no se transfieren al NAS.

El flujo de Compose del NAS es independiente, manual y está documentado en [deployment/nas-manual.md](deployment/nas-manual.md). Solo se ejecuta en el NAS desde un checkout seleccionado manualmente cuando existe una versión utilizable.

## Herramientas necesarias

- `dotnet`
- `docker` con Compose v2 y `buildx`
- `curl`

## Notas

- El primer comando configura `ConnectionStrings:Postgres` solo para una API ejecutada localmente en el entorno `Development`. No configura Compose.
- El script de verificación de Compose usa raíces fixture temporales y no toca la biblioteca musical de producción.
- La verificación de Compose no crea un contenedor PostgreSQL local y no puede leer .NET User Secrets. Usa un valor aislado de desarrollo/prueba para `ConnectionStrings__Postgres` fuera de Git mediante el entorno o el archivo `.env.local` ignorado.
- Ningún comando de WSL transfiere código, imágenes ni archivos al NAS.
