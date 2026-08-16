# Local WSL Gate

Run development verification from WSL with the working tree on the Linux filesystem.

## Commands

```bash
./scripts/wsl/build.sh
./scripts/wsl/test.sh
./scripts/wsl/compose-verify.sh
./scripts/wsl/build-images.sh
```

## What Each Command Does

- `build.sh` restores and builds the solution in Release mode.
- `test.sh` runs the unit and integration tests in Release mode.
- `compose-verify.sh` starts the Compose stack against disposable local roots and an ephemeral PostgreSQL volume, then checks the API health endpoint and the Web root page.
- `build-images.sh` builds the API and Web images for `linux/amd64` from the exact current Git commit and tags them with that full commit SHA.

## Required Tools

- `dotnet`
- `docker` with Compose v2 and `buildx`
- `curl`

## Notes

- The Compose verification script uses temporary fixture roots and does not touch the production music library.
- The image build script refuses a dirty worktree so the commit tag always matches a clean source state.
