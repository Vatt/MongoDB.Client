using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    public enum CollationCaseFirst
    {
        [BsonElement("lower")]
        Lower,
        [BsonElement("off")]
        Off,
        [BsonElement("upper")]
        Upper
    }

    public enum CollationAlternate
    {
        [BsonElement("non-ignorable")]
        NonIgnorable,
        [BsonElement("shifted")]
        Shifted
    }

    public enum CollationStrength : int
    {
        Primary = 1,
        Secondary = 2,
        Tertiary = 3,
        Quaternary = 4,
        Identical = 5
    }

    public enum CollationMaxVariable
    {
        [BsonElement("punct")]
        Punctuation,
        [BsonElement("space")]
        Space,
    }
    [BsonSerializable]
    public sealed partial class Collation
    {
        [BsonElement("locale")]
        public string Locale { get; init; }

        [BsonElement("caseLevel")]
        public bool? CaseLevel { get; init; }

        [BsonElement("caseFirst")]
        [BsonEnum(EnumRepresentation.String)]
        public CollationCaseFirst CaseFirst { get; init; }

        [BsonElement("strength")]
        [BsonEnum(EnumRepresentation.Int32)]
        public CollationStrength Strength { get; init; }

        [BsonElement("numericOrdering")]
        public bool? NumericOrdering { get; init; }

        [BsonElement("alternate")]
        [BsonEnum(EnumRepresentation.String)]
        public CollationAlternate Alternate { get; init; }

        [BsonElement("maxVariable")]
        [BsonEnum(EnumRepresentation.String)]
        public CollationMaxVariable MaxVariable { get; init; }

        [BsonElement("backwards")]
        public bool? Backwards { get; init; }

        public Collation(string locale, bool? caseLevel, CollationCaseFirst caseFirst, CollationStrength strength, bool? numericOrdering, CollationAlternate alternate, CollationMaxVariable maxVariable, bool? backwards)
        {
            Locale = locale;
            CaseLevel = caseLevel;
            CaseFirst = caseFirst;
            Strength = strength;
            NumericOrdering = numericOrdering;
            Alternate = alternate;
            MaxVariable = maxVariable;
            Backwards = backwards;
        }
    }
}
