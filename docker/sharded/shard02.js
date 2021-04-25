rs.initiate(
    {
        _id: 'rs-shard02',
        members: [
            {_id:0, host: "centos2.mshome.net:27020"},
            {_id:1, host: "centos2.mshome.net:27021"},
            {_id:2, host: "centos2.mshome.net:27022"}
        ]
    }
)