try {
    const status = rs.status();
    if (status.ok === 1) {
        print('Config replica set already initialized.');
        quit(0);
    }
} catch (e) {
}

rs.initiate(
    {
        _id: 'rs-cfg',
        configsvr: true,
        members: [
            { _id: 0, host: 'mongo-cfg-a:27017' },
            { _id: 1, host: 'mongo-cfg-b:27017' },
            { _id: 2, host: 'mongo-cfg-c:27017' }
        ]
    }
);
