const adminDb = db.getSiblingDB('admin');
const username = process.env.MONGO_INITDB_ROOT_USERNAME ?? 'root';
const password = process.env.MONGO_INITDB_ROOT_PASSWORD ?? 'password';

try {
    adminDb.createUser({
        user: username,
        pwd: password,
        roles: [
            { role: 'root', db: 'admin' }
        ]
    });
} catch (error) {
    if (`${error}`.includes('already exists')) {
        print(`Admin user '${username}' already exists.`);
        quit(0);
    }

    throw error;
}
