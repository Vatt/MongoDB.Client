namespace MongoDB.Client.Exceptions
{
    public class MongoBadClientTypeException : MongoException
    {
        public MongoBadClientTypeException() : base("Bad client type")
        {

        }
    }
}
