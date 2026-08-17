#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(git rev-parse --show-toplevel)"
PROJECT_NAME="djtracksessions-verify"
TEMP_ROOT="$(mktemp -d)"
LOCAL_ENV_FILE="${LOCAL_ENV_FILE:-${ROOT_DIR}/.env.local}"

cleanup() {
  if [[ -r "${LOCAL_ENV_FILE}" ]]; then
    docker compose --env-file "${LOCAL_ENV_FILE}" -f "${ROOT_DIR}/docker-compose.yml" -p "${PROJECT_NAME}" down -v --remove-orphans >/dev/null 2>&1 || true
  else
    docker compose -f "${ROOT_DIR}/docker-compose.yml" -p "${PROJECT_NAME}" down -v --remove-orphans >/dev/null 2>&1 || true
  fi
  rm -rf "${TEMP_ROOT}"
}

trap cleanup EXIT

export MAIN_LIBRARY_PATH="${TEMP_ROOT}/main"
export PENDING_LIBRARY_PATH="${TEMP_ROOT}/pending"
export REMEMBER_LIBRARY_PATH="${TEMP_ROOT}/remember"
export SESSIONS_LIBRARY_PATH="${TEMP_ROOT}/sessions"
if [[ -z "${ConnectionStrings__Postgres:-}" && ! -r "${LOCAL_ENV_FILE}" ]]; then
  printf '%s\n' "Compose requires ConnectionStrings__Postgres through the environment or ${LOCAL_ENV_FILE}; .NET User Secrets are not available in containers." >&2
  exit 1
fi

mkdir -p "${MAIN_LIBRARY_PATH}" "${PENDING_LIBRARY_PATH}" "${REMEMBER_LIBRARY_PATH}" "${SESSIONS_LIBRARY_PATH}"

compose_args=(-f "${ROOT_DIR}/docker-compose.yml" -p "${PROJECT_NAME}")

if [[ -r "${LOCAL_ENV_FILE}" ]]; then
  compose_args=(--env-file "${LOCAL_ENV_FILE}" "${compose_args[@]}")
fi

docker compose "${compose_args[@]}" config >/dev/null
docker compose "${compose_args[@]}" up --build --wait

curl -fsS "http://localhost:8080/health" >/dev/null
curl -fsS "http://localhost:8081/" >/dev/null
