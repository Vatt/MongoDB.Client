using MongoDB.Client.Messages;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Client.Exceptions
{
    public class MongoInsertException : MongoException
    {
        public List<InsertError>? Errors { get; }

        public MongoInsertException(List<InsertError> errors) : base(errors.FirstOrDefault()?.ErrorMessage)
        {
            Errors = errors;
        }

        public MongoInsertException(string error) : base(error)
        {

        }
    }
}