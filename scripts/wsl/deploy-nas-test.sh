#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(git rev-parse --show-toplevel)"

NAS_HOST="${NAS_HOST:?NAS_HOST is required}"
NAS_USER="${NAS_USER:?NAS_USER is required}"
NAS_DEPLOY_DIR="${NAS_DEPLOY_DIR:?NAS_DEPLOY_DIR is required}"
IMAGE_TAG="${IMAGE_TAG:?IMAGE_TAG is required}"
NAS_MAIN_LIBRARY_PATH="${NAS_MAIN_LIBRARY_PATH:?NAS_MAIN_LIBRARY_PATH is required}"
NAS_PENDING_LIBRARY_PATH="${NAS_PENDING_LIBRARY_PATH:?NAS_PENDING_LIBRARY_PATH is required}"
NAS_REMEMBER_LIBRARY_PATH="${NAS_REMEMBER_LIBRARY_PATH:?NAS_REMEMBER_LIBRARY_PATH is required}"
NAS_SESSIONS_LIBRARY_PATH="${NAS_SESSIONS_LIBRARY_PATH:?NAS_SESSIONS_LIBRARY_PATH is required}"
NAS_APP_DATA_PATH="${NAS_APP_DATA_PATH:?NAS_APP_DATA_PATH is required}"
NAS_POSTGRES_DATA_PATH="${NAS_POSTGRES_DATA_PATH:?NAS_POSTGRES_DATA_PATH is required}"
API_IMAGE_NAME="${API_IMAGE_NAME:-djtracksessions-api}"
WEB_IMAGE_NAME="${WEB_IMAGE_NAME:-djtracksessions-web}"
MIGRATIONS_IMAGE_NAME="${MIGRATIONS_IMAGE_NAME:-djtracksessions-api-migrations}"
POSTGRES_DB="${POSTGRES_DB:-djtracksessions}"
POSTGRES_USER="${POSTGRES_USER:-djtracksessions}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:?POSTGRES_PASSWORD is required}"
REMOTE_COMPOSE_FILE="${REMOTE_COMPOSE_FILE:-docker-compose.nas-test.yml}"
BASE_COMPOSE_FILE="${BASE_COMPOSE_FILE:-docker-compose.yml}"
BACKUP_NAME="${BACKUP_NAME:-postgres-pre-migration-${IMAGE_TAG}.sql}"
API_HEALTH_URL="${API_HEALTH_URL:-http://localhost:8080/health}"
WEB_HEALTH_URL="${WEB_HEALTH_URL:-http://localhost:8081/}"
SMOKE_URL="${SMOKE_URL:-http://localhost:8081/}"

ssh_nas() {
  ssh "${NAS_USER}@${NAS_HOST}" "$1"
}

tmp_dir="$(mktemp -d)"
cleanup() {
  rm -rf "${tmp_dir}"
}
trap cleanup EXIT

api_archive="${tmp_dir}/djtracksessions-api-${IMAGE_TAG}.tar"
web_archive="${tmp_dir}/djtracksessions-web-${IMAGE_TAG}.tar"
migrations_archive="${tmp_dir}/djtracksessions-api-migrations-${IMAGE_TAG}.tar"

docker save "${API_IMAGE_NAME}:${IMAGE_TAG}" -o "${api_archive}"
docker save "${WEB_IMAGE_NAME}:${IMAGE_TAG}" -o "${web_archive}"
docker save "${MIGRATIONS_IMAGE_NAME}:${IMAGE_TAG}" -o "${migrations_archive}"

ssh_nas "mkdir -p '${NAS_DEPLOY_DIR}'"
scp "${ROOT_DIR}/${BASE_COMPOSE_FILE}" "${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_DIR}/${BASE_COMPOSE_FILE}"
scp "${api_archive}" "${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_DIR}/"
scp "${web_archive}" "${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_DIR}/"
scp "${migrations_archive}" "${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_DIR}/"
scp "${ROOT_DIR}/docker-compose.nas-test.yml" "${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_DIR}/${REMOTE_COMPOSE_FILE}"

ssh_nas "docker load -i '${NAS_DEPLOY_DIR}/$(basename "${api_archive}")'"
ssh_nas "docker load -i '${NAS_DEPLOY_DIR}/$(basename "${web_archive}")'"
ssh_nas "docker load -i '${NAS_DEPLOY_DIR}/$(basename "${migrations_archive}")'"

COMPOSE_ENV="NAS_MAIN_LIBRARY_PATH='${NAS_MAIN_LIBRARY_PATH}' NAS_PENDING_LIBRARY_PATH='${NAS_PENDING_LIBRARY_PATH}' NAS_REMEMBER_LIBRARY_PATH='${NAS_REMEMBER_LIBRARY_PATH}' NAS_SESSIONS_LIBRARY_PATH='${NAS_SESSIONS_LIBRARY_PATH}' NAS_APP_DATA_PATH='${NAS_APP_DATA_PATH}' NAS_POSTGRES_DATA_PATH='${NAS_POSTGRES_DATA_PATH}' POSTGRES_DB='${POSTGRES_DB}' POSTGRES_USER='${POSTGRES_USER}' POSTGRES_PASSWORD='${POSTGRES_PASSWORD}' IMAGE_TAG='${IMAGE_TAG}' API_IMAGE_NAME='${API_IMAGE_NAME}' WEB_IMAGE_NAME='${WEB_IMAGE_NAME}' MIGRATIONS_IMAGE_NAME='${MIGRATIONS_IMAGE_NAME}'"
COMPOSE_FILES="-f '${BASE_COMPOSE_FILE}' -f '${REMOTE_COMPOSE_FILE}'"

ssh_nas "cd '${NAS_DEPLOY_DIR}' && ${COMPOSE_ENV} docker compose ${COMPOSE_FILES} -p djtracksessions-test config >/dev/null"

ssh_nas "cd '${NAS_DEPLOY_DIR}' && ${COMPOSE_ENV} docker compose ${COMPOSE_FILES} -p djtracksessions-test up -d postgres"

ssh_nas "cd '${NAS_DEPLOY_DIR}' && ${COMPOSE_ENV} docker compose ${COMPOSE_FILES} -p djtracksessions-test exec -T postgres pg_dump -U '${POSTGRES_USER}' -d '${POSTGRES_DB}' > '${BACKUP_NAME}'"

ssh_nas "cd '${NAS_DEPLOY_DIR}' && ${COMPOSE_ENV} docker compose ${COMPOSE_FILES} -p djtracksessions-test run --rm migrations"

ssh_nas "cd '${NAS_DEPLOY_DIR}' && ${COMPOSE_ENV} docker compose ${COMPOSE_FILES} -p djtracksessions-test up -d --no-build api web"

ssh_nas "curl -fsS '${API_HEALTH_URL}' >/dev/null"
ssh_nas "curl -fsS '${WEB_HEALTH_URL}' >/dev/null"
ssh_nas "curl -fsS '${SMOKE_URL}' >/dev/null"
