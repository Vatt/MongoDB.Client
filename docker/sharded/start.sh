#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
compose_file="${script_dir}/docker-compose.yml"
mongo_user="${MONGO_INITDB_ROOT_USERNAME:-root}"
mongo_password="${MONGO_INITDB_ROOT_PASSWORD:-password}"
encoded_mongo_user=""
encoded_mongo_password=""

uri_encode() {
    local value="$1"

    docker exec \
        -e MONGO_URI_COMPONENT="${value}" \
        mongo-router01 \
        mongosh --quiet --nodb --eval '
            const value = process.env.MONGO_URI_COMPONENT ?? "";
            const encoded = encodeURIComponent(value).replace(/[!'"'"'()*]/g, (char) =>
                `%${char.charCodeAt(0).toString(16).toUpperCase()}`
            );
            console.log(encoded);
        '
}

wait_for_local_ping() {
    local container_name="$1"

    until docker exec "${container_name}" mongosh --quiet --eval "db.adminCommand({ ping: 1 }).ok" >/dev/null 2>&1; do
        sleep 2
    done
}

wait_for_primary() {
    local replica_set_name="$1"
    shift
    local container_names=("$@")
    local container_name

    until
        for container_name in "${container_names[@]}"; do
            if docker exec "${container_name}" mongosh --quiet --eval "const hello = db.hello(); if (hello.setName !== '${replica_set_name}') { quit(1); } if (!(hello.isWritablePrimary || hello.ismaster)) { quit(1); }" >/dev/null 2>&1; then
                return 0
            fi
        done

        false
    do
        sleep 2
    done
}

wait_for_authenticated_router_ping() {
    until docker run --rm --add-host host.docker.internal:host-gateway mongo:8.0-noble \
        mongosh "mongodb://${encoded_mongo_user}:${encoded_mongo_password}@host.docker.internal:27029/admin?authSource=admin&authMechanism=SCRAM-SHA-256" \
        --quiet --eval "db.adminCommand({ ping: 1 }).ok" >/dev/null 2>&1; do
        sleep 2
    done
}

run_mongod_script() {
    local container_name="$1"
    local script_name="$2"

    if docker exec "${container_name}" mongosh --quiet -u "${mongo_user}" -p "${mongo_password}" --authenticationDatabase admin --eval "db.adminCommand({ ping: 1 }).ok" >/dev/null 2>&1; then
        docker exec -i "${container_name}" mongosh --quiet -u "${mongo_user}" -p "${mongo_password}" --authenticationDatabase admin < "${script_dir}/${script_name}"
        return
    fi

    docker exec -i "${container_name}" mongosh --quiet < "${script_dir}/${script_name}"
}

router_auth_available() {
    docker exec mongo-router01 mongosh --quiet -u "${mongo_user}" -p "${mongo_password}" --authenticationDatabase admin --eval "db.adminCommand({ ping: 1 }).ok" >/dev/null 2>&1
}

run_router_script() {
    local script_name="$1"

    if router_auth_available; then
        docker exec -i mongo-router01 mongosh --quiet -u "${mongo_user}" -p "${mongo_password}" --authenticationDatabase admin < "${script_dir}/${script_name}"
        return
    fi

    docker exec -i mongo-router01 mongosh --quiet < "${script_dir}/${script_name}"
}

docker compose -f "${compose_file}" up -d

wait_for_local_ping mongo-cfg-a
wait_for_local_ping mongo-shard01-a
wait_for_local_ping mongo-shard02-a
wait_for_local_ping mongo-shard03-a

run_mongod_script mongo-cfg-a cfg.js
run_mongod_script mongo-shard01-a shard01.js
run_mongod_script mongo-shard02-a shard02.js
run_mongod_script mongo-shard03-a shard03.js

wait_for_primary rs-cfg mongo-cfg-a mongo-cfg-b mongo-cfg-c
wait_for_primary rs-shard01 mongo-shard01-a mongo-shard01-b mongo-shard01-c
wait_for_primary rs-shard02 mongo-shard02-a mongo-shard02-b mongo-shard02-c
wait_for_primary rs-shard03 mongo-shard03-a mongo-shard03-b mongo-shard03-c

wait_for_local_ping mongo-router01

encoded_mongo_user="$(uri_encode "${mongo_user}")"
encoded_mongo_password="$(uri_encode "${mongo_password}")"

run_router_script router.js
if ! router_auth_available; then
    docker exec \
        -e MONGO_INITDB_ROOT_USERNAME="${mongo_user}" \
        -e MONGO_INITDB_ROOT_PASSWORD="${mongo_password}" \
        -i mongo-router01 mongosh --quiet < "${script_dir}/root-user.js"
fi

wait_for_authenticated_router_ping

cat <<EOF
Sharded MongoDB cluster is ready.
Router endpoint: localhost:27029
Connection string: mongodb://${encoded_mongo_user}:${encoded_mongo_password}@localhost:27029/?maxPoolSize=1&authSource=admin&authMechanism=SCRAM-SHA-256
EOF
