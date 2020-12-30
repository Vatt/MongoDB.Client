using System;
using System.Collections.Immutable;
using System.Net;

namespace MongoDB.Client
{
    public class MongoClientSettings
    {
        public MongoClientSettings(EndPoint[] endpoints, string login, string password)
        {
            // if (endpoints is {Length: 0})
            // {
            //     throw new ArgumentException("Endpoints must not be empty");
            // }
            Login = login;
            Password = password;
            Endpoints = ImmutableArray.Create(endpoints);
        }

        public MongoClientSettings(EndPoint[] endpoints)
        : this(endpoints, string.Empty, string.Empty)
        {
        }

        public MongoClientSettings(EndPoint endpoint)
            : this(new[] { endpoint }, string.Empty, string.Empty)
        {
        }

        public MongoClientSettings()
            : this(new[] { new IPEndPoint(IPAddress.Loopback, 27017) }, string.Empty, string.Empty)
        {
        }

        private readonly ImmutableArray<EndPoint> _endpoints;
        public ImmutableArray<EndPoint> Endpoints
        {
            get => _endpoints;
            init
            {
                if (value is { Length: 0 })
                {
                    throw new ArgumentException("Endpoints must not be empty");
                }

                _endpoints = value;
            }
        }

        public string Login { get; init; }
        public string Password { get; init; }

        public int ConnectionPoolMaxSize { get; init; } = 16;
        public int MultiplexingTreshold { get; init; } = 2;
    }
}