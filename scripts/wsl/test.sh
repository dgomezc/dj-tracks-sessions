#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(git rev-parse --show-toplevel)"

dotnet test "${ROOT_DIR}/DjTracksSessions.slnx" -c Release
