const shardHosts = [
    'rs-shard01/mongo-shard01-a:27017,mongo-shard01-b:27017,mongo-shard01-c:27017',
    'rs-shard02/mongo-shard02-a:27017,mongo-shard02-b:27017,mongo-shard02-c:27017',
    'rs-shard03/mongo-shard03-a:27017,mongo-shard03-b:27017,mongo-shard03-c:27017'
];

let existingShards = new Set();

try {
    const listShardsResult = db.adminCommand({ listShards: 1 });
    existingShards = new Set((listShardsResult.shards ?? []).map(shard => shard.host));
} catch (error) {
    print(`Falling back to addShard bootstrap mode: ${error}`);
}

for (const shardHost of shardHosts) {
    if (existingShards.has(shardHost)) {
        print(`Shard ${shardHost} already added.`);
        continue;
    }

    try {
        sh.addShard(shardHost);
    } catch (error) {
        if (`${error}`.includes('already exists')) {
            print(`Shard ${shardHost} already added.`);
            continue;
        }

        throw error;
    }
}
