rs.initiate(
    {
        _id: 'rs-shard01',
        members: [
            {_id:0, host: "mongo2.mshome.net:27017"},
            {_id:1, host: "mongo2.mshome.net:27018"},
            {_id:2, host: "mongo2.mshome.net:27019"}
        ]
    }
)