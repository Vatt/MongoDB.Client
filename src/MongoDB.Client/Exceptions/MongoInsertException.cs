using System.Collections.Generic;
using System.Linq;
using MongoDB.Client.Messages;

namespace MongoDB.Client.Exceptions
{
    public class MongoInsertException : MongoException
    {
        public List<InsertError> Errors { get; }

        public MongoInsertException(List<InsertError> errors) : base(errors.FirstOrDefault()?.ErrorMessage)
        {
            Errors = errors;
        }
    }
}