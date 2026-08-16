# DJ Tracks & Sessions

Web application for cataloging, tagging, organizing, and playing a personal electronic music collection stored on a NAS.

The application is designed for a single user, runs exclusively in Docker, and is accessed from a desktop browser on the local network.

Canonical repository: <https://github.com/dgomezc/dj-tracks-and-sessions>

## Status

Planning phase. Implementation has not started.

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

## Library Roots

Docker Compose will mount four independently configured roots:

| Root | Purpose |
|---|---|
| Main library | Cataloged tracks organized by personal year and genre |
| Pending | Inbox for tracks awaiting analysis and approval |
| Remember | Older tracks that remain in place and always use `PersonalGenre=Remember` |
| Sessions | Personal DJ sessions and their TXT tracklists |

## Execution Rule

Implement [PLAN.md](PLAN.md) in order. Complete and verify one phase before starting the next. Do not implement future items as part of the MVP unless the plan is explicitly amended.

Development runs from a GitHub feature branch in the WSL Linux filesystem. Local build, test, and Compose verification is the delivery gate; test deployments use locally built immutable images transferred directly to the NAS. See [PLAN.md, Development And Test Deployment Workflow](PLAN.md#151-development-and-test-deployment-workflow).

For the repeatable WSL gate commands, see [docs/local-gate.md](docs/local-gate.md).
