#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
mongo_user="${MONGO_INITDB_ROOT_USERNAME:-root}"
mongo_password="${MONGO_INITDB_ROOT_PASSWORD:-password}"

docker compose -f "${script_dir}/docker-compose.yml" up -d

until docker exec mongo-rs0 mongosh --quiet -u "${mongo_user}" -p "${mongo_password}" --authenticationDatabase admin --eval "db.adminCommand({ ping: 1 }).ok" >/dev/null 2>&1; do
    sleep 2
done

docker exec -i mongo-rs0 mongosh --quiet -u "${mongo_user}" -p "${mongo_password}" --authenticationDatabase admin < "${script_dir}/init.js"
