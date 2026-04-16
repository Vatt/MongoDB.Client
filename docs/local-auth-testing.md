# Local Auth Testing

Короткая памятка для локальной проверки auth-related изменений в `MongoDB.Client`.

## Local Stacks

### Standalone with auth

Start:

```bash
./docker/single/start.sh
```

Stop:

```bash
./docker/single/down.sh
```

Defaults:

- endpoint: `localhost:27016`
- username: `root`
- password: `password`
- auth source: `admin`
- auth mechanism: `SCRAM-SHA-256`

Example connection string:

```text
mongodb://root:password@localhost:27016/?maxPoolSize=1&authSource=admin&authMechanism=SCRAM-SHA-256
```

### Replica set with auth

Start:

```bash
./docker/replSet/start.sh
```

Stop:

```bash
./docker/replSet/down.sh
```

Defaults:

- endpoint: `localhost:27017`
- replica set: `rs0`
- username: `root`
- password: `password`
- auth source: `admin`
- auth mechanism: `SCRAM-SHA-256`

Example connection string:

```text
mongodb://root:password@localhost:27017/?maxPoolSize=1&authSource=admin&authMechanism=SCRAM-SHA-256&replicaSet=rs0
```

## Test Infrastructure Defaults

`tests/MongoDB.Client.Tests/Infrastructure/IntegrationMongoConnectionStringBuilder.cs` использует такие env vars:

- `MONGODB_HOST`: standalone host, по умолчанию `localhost:27016`
- `MONGODB_RS_HOST`: replica set host, по умолчанию `localhost:27017`
- `MONGODB_SHARDED_HOST`: sharded router host, по умолчанию `localhost:27029`
- `MONGODB_RS_NAME`: replica set name, по умолчанию `rs0`
- `MONGODB_REPLICA_SET`: fallback alias для replica set name, если `MONGODB_RS_NAME` не задан
- `MONGODB_USERNAME`: username, по умолчанию `root`
- `MONGODB_PASSWORD`: password, по умолчанию `password`
- `MONGODB_AUTH_SOURCE`: auth database, по умолчанию `admin`
- `MONGODB_AUTH_MECHANISM`: auth mechanism, по умолчанию `SCRAM-SHA-256`

Замечание: builder всегда добавляет `maxPoolSize`, а `authSource` и `authMechanism` добавляются автоматически, если задан `username`.

Credential env vars у docker stack и test builder разные:

- docker stacks: `MONGO_INITDB_ROOT_USERNAME` / `MONGO_INITDB_ROOT_PASSWORD`
- test builder: `MONGODB_USERNAME` / `MONGODB_PASSWORD`

Если поменять только `MONGO_INITDB_ROOT_*` и не обновить matching `MONGODB_*`, тесты продолжат собирать connection string со старыми credential values и это выглядит как auth regression, хотя проблема инфраструктурная.

## Auth Checks Before Merge

Для auth-related изменений минимум полезно прогонять:

```bash
dotnet test MongoDB.Client.sln --filter FullyQualifiedName~ScramAuthenticatorTests
```

```bash
dotnet test MongoDB.Client.sln --filter FullyQualifiedName~AuthenticationMongoIntegrationTests
```

```bash
dotnet test MongoDB.Client.sln --filter FullyQualifiedName~MongoClientDiagnosticsTests
```

```bash
dotnet test MongoDB.Client.sln --filter FullyQualifiedName~MongoSchedulerReconnectTests
```

Практически:

- `ScramAuthenticatorTests` покрывают SCRAM handshake и negotiation logic
- `AuthenticationMongoIntegrationTests` проверяют auth against live standalone and replica set stacks
- `MongoClientDiagnosticsTests` и `MongoSchedulerReconnectTests` полезны как минимальный regression check вокруг diagnostics/reconnect behavior, который часто затрагивается рядом с auth/connectivity changes

## Sharded Stack

Поднять sharded стенд можно так:

```bash
./docker/sharded/start.sh
```

Остановить:

```bash
./docker/sharded/down.sh
```

Default router endpoint:

- `localhost:27029`

Для auth-related локальной проверки важно считать sharded stack отдельным случаем:

- shipped sharded stack не является working auth target для локальной auth-проверки
- test builder по умолчанию умеет собирать auth connection string для `MONGODB_SHARDED_HOST`, но это не означает, что текущий sharded stack пригоден как auth validation target
- sharded stack можно использовать только для environment-dependent topology/manual checks
- текущая конфигурация жёстко завязана на `mongo2.mshome.net`, поэтому даже manual checks зависят от локального окружения
