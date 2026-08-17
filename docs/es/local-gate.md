# Puerta local de WSL

> Traducción al español. El documento original en inglés se conserva en `../../docs/local-gate.md`.

Ejecuta la verificación del desarrollo desde WSL con el árbol de trabajo en el sistema de archivos Linux.

## Comandos

```bash
./scripts/wsl/build.sh
./scripts/wsl/test.sh
./scripts/wsl/compose-verify.sh
./scripts/wsl/build-images.sh
```

## Qué hace cada comando

- `build.sh` restaura y compila la solución en modo Release.
- `test.sh` ejecuta las pruebas unitarias y de integración en modo Release.
- `compose-verify.sh` inicia el stack de Compose contra raíces locales desechables y un volumen PostgreSQL efímero; después comprueba el endpoint de salud de la API y la página raíz de Web.
- `build-images.sh` construye las imágenes de API, Web y migraciones para `linux/amd64` desde el commit Git actual exacto y las etiqueta con el SHA completo de ese commit.

El flujo de despliegue en el NAS es independiente y está documentado en [deployment/nas-test.md](deployment/nas-test.md).

## Herramientas necesarias

- `dotnet`
- `docker` con Compose v2 y `buildx`
- `curl`

## Notas

- El script de verificación de Compose usa raíces fixture temporales y no toca la biblioteca musical de producción.
- El script de construcción de imágenes rechaza un árbol de trabajo sucio para que la etiqueta del commit siempre coincida con un estado limpio del código fuente.
