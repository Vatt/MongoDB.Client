#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

docker compose -f "${script_dir}/docker-compose.yml" down --remove-orphans
