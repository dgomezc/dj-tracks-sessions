# DJ Tracks & Sessions

> Traducción al español. El documento original en inglés se conserva en `../../README.md`.

Aplicación web para catalogar, etiquetar, organizar y reproducir una colección personal de música electrónica almacenada en un NAS.

La aplicación está diseñada para un solo usuario, se ejecuta exclusivamente en Docker y se accede desde un navegador de escritorio en la red local.

Repositorio canónico: <https://github.com/dgomezc/dj-tracks-and-sessions>

## Estado

La implementación de la base está en curso. La etapa inmediata oficial es la [Fase 2A: Entrega con prioridad al catálogo](PLAN.md#fase-2a-entrega-con-prioridad-al-catalogo), después de las unidades 1-4 completadas de la Fase 2.

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

## Configuración de base de datos

La instancia PostgreSQL del NAS es la base de datos de desarrollo. Este repositorio despliega solo contenedores de aplicación: nunca crea, inicia, monta ni gestiona un contenedor o volumen de base de datos PostgreSQL.

Para el desarrollo local de la API con el entorno `Development`, configura `ConnectionStrings:Postgres` en .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:Postgres" "<connection-string>" --project src/DjTracksSessions.Api
```

User Secrets se usa solo en desarrollo local y no está disponible dentro de contenedores. Docker Compose y los despliegues en NAS deben proporcionar `ConnectionStrings__Postgres` mediante un archivo de entorno o secretos externo e ignorado. No incluyas datos de conexión en Git.

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

El desarrollo se ejecuta desde una rama de funcionalidades de GitHub en el sistema de archivos Linux de WSL. La compilación, las pruebas y las comprobaciones opcionales de imágenes `linux/amd64` locales son la puerta de entrega; las imágenes locales no se transfieren al NAS. Las pruebas de Compose en el NAS solo se realizan cuando una versión utilizable se clona y selecciona manualmente en el NAS. Consulta [PLAN.md, Flujo de desarrollo y despliegue de pruebas](PLAN.md#151-development-and-test-deployment-workflow).

Para los comandos repetibles de la puerta de WSL, consulta [docs/local-gate.md](local-gate.md).

Para el flujo manual de despliegue de prueba en el NAS, consulta [docs/deployment/nas-manual.md](deployment/nas-manual.md).

Para el desarrollo local de la API y las pruebas con Scalar, consulta [api-development.md](api-development.md).
