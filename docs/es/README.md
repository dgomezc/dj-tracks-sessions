# DJ Tracks & Sessions

> Traducción al español. El documento original en inglés se conserva en `../../README.md`.

Aplicación web para catalogar, etiquetar, organizar y reproducir una colección personal de música electrónica almacenada en un NAS.

La aplicación está diseñada para un solo usuario, se ejecuta exclusivamente en Docker y se accede desde un navegador de escritorio en la red local.

Repositorio canónico: <https://github.com/dgomezc/dj-tracks-and-sessions>

## Estado

Fase de planificación. La implementación aún no ha comenzado.

Lee estos documentos antes de modificar el proyecto:

1. [AGENTS.md](AGENTS.md): reglas de ingeniería obligatorias.
2. [PLAN.md](PLAN.md): requisitos del producto, arquitectura y fases de implementación ordenadas.
3. [DECISIONS.md](DECISIONS.md): decisiones de producto y arquitectura que no deben cambiarse silenciosamente.

## Stack previsto

- .NET 10 y ASP.NET Core Web API.
- Blazor Web App con renderizado Interactive Server.
- Blazor Blueprint UI.
- Arquitectura Vertical Slice.
- FluentResults y FluentValidation.
- Entity Framework Core con PostgreSQL mediante Npgsql.
- MusicBrainz, Discogs, AcoustID, Chromaprint y FFmpeg.
- Docker Compose en un NAS Linux x86-64.

`DjTracksSessions.Api` es propietaria de `Features` organizadas como vertical slices, incluidos endpoints, consultas, comandos, validaciones, mappers, handlers y pruebas de comportamiento. Los slices usan entidades e invariantes independientes del framework de `DjTrackSessions.Domain`; `DjTrackSessions.Infrastructure` es propietaria de EF Core Code First, las configuraciones Fluent, las migraciones y los adaptadores externos.

## Raíces de la biblioteca

Docker Compose montará cuatro raíces configuradas de forma independiente:

| Raíz | Propósito |
|---|---|
| Main library | Pistas catalogadas organizadas por año personal y género |
| Pending | Bandeja de entrada para pistas pendientes de análisis y aprobación |
| Remember | Pistas antiguas que permanecen en su lugar y siempre usan `PersonalGenre=Remember` |
| Sessions | Sesiones personales de DJ y sus tracklists TXT |

## Regla de ejecución

Implementa [PLAN.md](PLAN.md) en orden. Completa y verifica una fase antes de comenzar la siguiente. No implementes elementos futuros como parte del MVP salvo que el plan se modifique explícitamente.

El desarrollo se ejecuta desde una rama de funcionalidades de GitHub en el sistema de archivos Linux de WSL. La compilación, las pruebas y la verificación de Compose locales son la puerta de entrega; los despliegues de prueba usan imágenes inmutables construidas localmente y transferidas directamente al NAS. Consulta [PLAN.md, Flujo de desarrollo y despliegue de pruebas](PLAN.md#151-development-and-test-deployment-workflow).

Para los comandos repetibles de la puerta de WSL, consulta [docs/local-gate.md](local-gate.md).

Para el flujo de despliegue de prueba en el NAS, consulta [docs/deployment/nas-test.md](deployment/nas-test.md).
