docker-compose up -d;
sleep 10s;

docker exec -i mongo-shard01-a mongo < shard01.js;
docker exec -i mongo-shard02-a mongo < shard02.js;
docker exec -i mongo-shard03-a mongo < shard03.js;

docker exec -i mongo-cfg-a mongo < cfg.js;

docker exec -i mongo-router01 mongo < router.js;