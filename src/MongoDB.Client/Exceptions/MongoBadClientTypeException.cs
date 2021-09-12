using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Exceptions
{
    public class MongoBadClientTypeException : MongoException
    {
        public MongoBadClientTypeException() : base("Bad client type")
        {

        }
    }
}
