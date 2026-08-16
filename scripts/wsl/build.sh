#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(git rev-parse --show-toplevel)"

dotnet restore "${ROOT_DIR}/DjTracksSessions.slnx"
dotnet build "${ROOT_DIR}/DjTracksSessions.slnx" -c Release --no-restore
