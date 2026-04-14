#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

docker compose -f "${script_dir}/docker-compose.yml" up -d
sleep 5
docker exec -i mongo-cfg-a mongosh --quiet < "${script_dir}/cfg.js"

docker exec -i mongo-shard01-a mongosh --quiet < "${script_dir}/shard01.js"
docker exec -i mongo-shard02-a mongosh --quiet < "${script_dir}/shard02.js"
docker exec -i mongo-shard03-a mongosh --quiet < "${script_dir}/shard03.js"

sleep 60
docker exec -i mongo-router01 mongosh --quiet < "${script_dir}/router.js"
