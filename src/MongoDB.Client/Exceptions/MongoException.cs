using System;

namespace MongoDB.Client.Exceptions
{
    public class MongoException : Exception
    {
        public MongoException(string? message) : base(message)
        {

        }
    }
}