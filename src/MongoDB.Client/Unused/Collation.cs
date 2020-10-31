using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Unused
{
    public class Collation
    {
        public string? Locale { get; set; }



        public BsonDocument ToBsonDocument()
        {
            return null!;

            //return new BsonDocument
            //{
            //    { "locale", _locale },
            //    { "caseLevel", () => _caseLevel.Value, _caseLevel.HasValue },
            //    { "caseFirst", () => ToString(_caseFirst.Value), _caseFirst.HasValue },
            //    { "strength", () => ToInt32(_strength.Value), _strength.HasValue },
            //    { "numericOrdering", () => _numericOrdering.Value, _numericOrdering.HasValue },
            //    { "alternate", () => ToString(_alternate.Value), _alternate.HasValue },
            //    { "maxVariable", () => ToString(_maxVariable.Value), _maxVariable.HasValue },
            //    { "normalization", () => _normalization.Value, _normalization.HasValue },
            //    { "backwards", () => _backwards.Value, _backwards.HasValue }
            //};
        }
    }
}
