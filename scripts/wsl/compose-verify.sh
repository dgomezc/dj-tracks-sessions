#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(git rev-parse --show-toplevel)"
PROJECT_NAME="djtracksessions-verify"
TEMP_ROOT="$(mktemp -d)"

cleanup() {
  docker compose -f "${ROOT_DIR}/docker-compose.yml" -p "${PROJECT_NAME}" down -v --remove-orphans >/dev/null 2>&1 || true
  rm -rf "${TEMP_ROOT}"
}

trap cleanup EXIT

export MAIN_LIBRARY_PATH="${TEMP_ROOT}/main"
export PENDING_LIBRARY_PATH="${TEMP_ROOT}/pending"
export REMEMBER_LIBRARY_PATH="${TEMP_ROOT}/remember"
export SESSIONS_LIBRARY_PATH="${TEMP_ROOT}/sessions"
export POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-djtracksessions-verify}"

mkdir -p "${MAIN_LIBRARY_PATH}" "${PENDING_LIBRARY_PATH}" "${REMEMBER_LIBRARY_PATH}" "${SESSIONS_LIBRARY_PATH}"

docker compose -f "${ROOT_DIR}/docker-compose.yml" -p "${PROJECT_NAME}" config >/dev/null
docker compose -f "${ROOT_DIR}/docker-compose.yml" -p "${PROJECT_NAME}" up --build --wait

curl -fsS "http://localhost:8080/health" >/dev/null
curl -fsS "http://localhost:8081/" >/dev/null
