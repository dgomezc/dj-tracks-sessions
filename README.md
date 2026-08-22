# DJ Tracks & Sessions

Web application for cataloging, tagging, organizing, and playing a personal electronic music collection stored on a NAS.

The application is designed for a single user, runs exclusively in Docker, and is accessed from a desktop browser on the local network. Track Management browses the configured Main, Pending, and Remember files directly; the file's tags are authoritative for what is displayed. PostgreSQL is not used for Track Management browsing, metadata editing, or Pending movement. Sessions remains a separate collection.

Canonical repository: <https://github.com/dgomezc/dj-tracks-and-sessions>

## Status

Foundation implementation is in progress. Track Management is filesystem-first: there is no persisted track index, scan endpoint, or persisted track ID. The file and its embedded tags are authoritative for browsing, detail, metadata editing, and Pending movement. PostgreSQL remains only for persistence with a real consumer, including settings, jobs, notifications, and Sessions where supported.

Read these documents before changing the project:

1. [AGENTS.md](AGENTS.md) - mandatory engineering rules.
2. [PLAN.md](PLAN.md) - product requirements, architecture, and ordered implementation phases.
3. [DECISIONS.md](DECISIONS.md) - product and architecture decisions that must not be silently changed.

## Intended Stack

- .NET 10 and ASP.NET Core Web API.
- Blazor Web App using Interactive Server rendering.
- Blazor Blueprint UI.
- Vertical Slice Architecture.
- FluentResults and FluentValidation.
- Entity Framework Core with PostgreSQL through Npgsql.
- MusicBrainz, Discogs, AcoustID, Chromaprint, and FFmpeg.
- Docker Compose on a Linux x86-64 NAS.

`DjTracksSessions.Api` owns vertical-slice `Features`, including endpoints, queries, commands, validations, mappers, handlers, and behavior tests. Slices use framework-independent entities and invariants from `DjTrackSessions.Domain`; `DjTrackSessions.Infrastructure` owns Code First EF Core, Fluent configurations, migrations, and external adapters.

## Database Configuration

The NAS PostgreSQL instance is the development database. This repository deploys application containers only: it never creates, starts, mounts, or owns a PostgreSQL container or database volume.

For local API development with the `Development` environment, provision `ConnectionStrings:Postgres` in .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:Postgres" "<connection-string>" --project src/DjTracksSessions.Api
```

User Secrets are local-development only and are not available inside containers. Docker Compose and NAS deployments must instead provide `ConnectionStrings__Postgres` through an ignored external environment or secret file. Do not put connection details in Git.

## Library Roots

Docker Compose will mount four independently configured roots:

| Root | Purpose |
|---|---|
| Main library | Cataloged tracks organized by personal year and genre |
| Pending | Inbox for tracks awaiting analysis and approval |
| Remember | Older tracks that remain in place and always use `PersonalGenre=Remember` |
| Sessions | Personal DJ sessions and their TXT tracklists |

Track Management resolves and lists audio files from these configured filesystem roots using relative paths confined to each root. It reads current technical properties and tags from each file; PostgreSQL is not the source required for browsing or tag display. Sessions is browsed through its own separate flow and is never included in Track Management.

## Execution Rule

Implement [PLAN.md](PLAN.md) in order. Close each screen, including its API behavior, UI states, safety flow, and tests, before opening the next: Track Management, Player, Sessions, then remaining screens. Do not implement future items as part of the MVP unless the plan is explicitly amended.

Development runs from a GitHub feature branch in the WSL Linux filesystem. Local build, test, and optional `linux/amd64` image-build checks are the delivery gate; locally built images are not transferred to the NAS. NAS Compose testing happens only when a usable version is manually cloned and checked out on the NAS. See [PLAN.md, Development And Test Deployment Workflow](PLAN.md#151-development-and-test-deployment-workflow).

For the repeatable WSL gate commands, see [docs/local-gate.md](docs/local-gate.md).

For the manual NAS test deployment flow, see [docs/deployment/nas-manual.md](docs/deployment/nas-manual.md).

For local API development and Scalar testing, see [docs/api-development.md](docs/api-development.md).

For Visual Studio Community 2026 F5 debugging of the API and Web together through WSL, see the [WSL debugging section](docs/api-development.md#debug-both-projects-from-visual-studio-with-wsl). Spanish documentation is available in [docs/es/api-development.md](docs/es/api-development.md#depurar-ambos-proyectos-desde-visual-studio-con-wsl).
