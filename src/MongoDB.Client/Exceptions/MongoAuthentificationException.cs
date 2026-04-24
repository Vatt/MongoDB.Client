using System;

namespace MongoDB.Client.Exceptions
{
    public class MongoAuthentificationException : MongoException
    {
        public MongoAuthentificationException(string? message, int code) : base($"{message} code: {code}")
        {

        }

        public MongoAuthentificationException(string? message, Exception ex) : base(message, ex)
        {

        }
    }
}
