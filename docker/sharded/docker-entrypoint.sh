#!/usr/bin/env bash
set -euo pipefail

cp /run/config/mongo-keyfile /tmp/mongo-keyfile
chmod 400 /tmp/mongo-keyfile
chown 999:999 /tmp/mongo-keyfile

exec "$@" --keyFile /tmp/mongo-keyfile
