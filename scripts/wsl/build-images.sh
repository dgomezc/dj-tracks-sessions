#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(git rev-parse --show-toplevel)"

if [[ -n "$(git status --porcelain)" ]]; then
  printf 'Refusing to build commit-tagged images from a dirty worktree.\n' >&2
  exit 1
fi

COMMIT_SHA="$(git rev-parse --verify HEAD)"
IMAGE_TAG="${COMMIT_SHA}"

docker buildx build \
  --platform linux/amd64 \
  --load \
  -t "djtracksessions-api:${IMAGE_TAG}" \
  -f "${ROOT_DIR}/src/DjTracksSessions.Api/Dockerfile" \
  "${ROOT_DIR}"

docker buildx build \
  --platform linux/amd64 \
  --load \
  -t "djtracksessions-web:${IMAGE_TAG}" \
  -f "${ROOT_DIR}/src/DjTracksSessions.Web/Dockerfile" \
  "${ROOT_DIR}"

docker buildx build \
  --platform linux/amd64 \
  --load \
  -t "djtracksessions-api-migrations:${IMAGE_TAG}" \
  -f "${ROOT_DIR}/src/DjTracksSessions.Api/Dockerfile" \
  --target migrations \
  "${ROOT_DIR}"
