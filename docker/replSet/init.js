rs.initiate(
    {
        _id: 'rs0',
        members: [
            {_id:0, host: "mongo-rs0:27017"}
        ]
    }
)
rs.add("mongo-rs1:27017")
rs.add("mongo-rs2:27017")