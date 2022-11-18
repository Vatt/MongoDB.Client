rs.initiate(
    {
        _id: 'rs0',
        members: [
            {_id:0, host: "host.docker.internal:27017"},
            {_id:1, host: "host.docker.internal:27018"},
            {_id:2, host: "host.docker.internal:27019"}
        ]
    }
)
