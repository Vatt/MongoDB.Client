rs.initiate(
    {
        _id: 'rs-shard01',
        members: [
            {_id:0, host: "centos2.mshome.net:27017"},
            {_id:1, host: "centos2.mshome.net:27018"},
            {_id:2, host: "centos2.mshome.net:27019"}
        ]
    }
)