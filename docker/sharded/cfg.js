rs.initiate(
    {
        _id: 'rs-cfg',
        configsvr: true,
        members: [
            {_id:0, host: "mongo2.mshome.net:27026"},
            {_id:1, host: "mongo2.mshome.net:27027"},
            {_id:2, host: "mongo2.mshome.net:27028"}
        ]
    }
)