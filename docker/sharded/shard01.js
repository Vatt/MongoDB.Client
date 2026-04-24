try {
    const status = rs.status();
    if (status.ok === 1) {
        print('Shard replica set rs-shard01 already initialized.');
        quit(0);
    }
} catch (e) {
}

rs.initiate(
    {
        _id: 'rs-shard01',
        members: [
            { _id: 0, host: 'mongo-shard01-a:27017' },
            { _id: 1, host: 'mongo-shard01-b:27017' },
            { _id: 2, host: 'mongo-shard01-c:27017' }
        ]
    }
);
