try {
    const status = rs.status();
    if (status.ok === 1) {
        print('Replica set already initialized.');
        quit(0);
    }
} catch (e) {
}

rs.initiate(
    {
        _id: 'rs0',
        members: [
            { _id: 0, host: 'localhost:27017' }
        ]
    }
);
