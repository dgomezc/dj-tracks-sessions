# Local WSL Gate

Run development verification from WSL with the working tree on the Linux filesystem.

## Commands

```bash
dotnet user-secrets set "ConnectionStrings:Postgres" "<connection-string>" --project src/DjTracksSessions.Api
./scripts/wsl/build.sh
./scripts/wsl/test.sh
./scripts/wsl/compose-verify.sh
./scripts/wsl/build-images.sh
```

## What Each Command Does

- `build.sh` restores and builds the solution in Release mode.
- `test.sh` runs the unit and integration tests in Release mode.
- `compose-verify.sh` is an optional local container check against disposable roots and an externally supplied non-versioned `ConnectionStrings__Postgres` value. It is not the NAS deployment workflow.
- `build-images.sh` optionally builds local `linux/amd64` API, Web, and migration images from the exact current Git commit. These images are not transferred to the NAS.

The NAS Compose workflow is separate, manual, and documented in [deployment/nas-manual.md](deployment/nas-manual.md). It runs only on the NAS from a manually selected checkout when a usable version exists.

## Required Tools

- `dotnet`
- `docker` with Compose v2 and `buildx`
- `curl`

## Notes

- The first command provisions `ConnectionStrings:Postgres` only for a locally run API in the `Development` environment. It does not configure Compose.
- The Compose verification script uses temporary fixture roots and does not touch the production music library.
- Compose verification does not create a local PostgreSQL container and cannot read .NET User Secrets. Use an isolated development/test `ConnectionStrings__Postgres` value supplied outside Git through the environment or ignored `.env.local` file.
- No WSL command transfers code, images, or archives to the NAS.
