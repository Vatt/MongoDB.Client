rs.initiate(
    {
        _id: 'rs-shard03',
        members: [
            {_id:0, host: "centos2.mshome.net:27023"},
            {_id:1, host: "centos2.mshome.net:27024"},
            {_id:2, host: "centos2.mshome.net:27025"}
        ]
    }
)