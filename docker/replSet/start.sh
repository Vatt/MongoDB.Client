docker-compose up -d;
sleep 5s;
docker exec -i mongo-rs0 mongo < init.js;
