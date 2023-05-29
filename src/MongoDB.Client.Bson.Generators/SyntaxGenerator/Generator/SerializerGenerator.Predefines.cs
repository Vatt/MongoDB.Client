using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly TypeSyntax BsonReaderType = SF.ParseTypeName("MongoDB.Client.Bson.Reader.BsonReader");
        public static readonly TypeSyntax BsonWriterType = SF.ParseTypeName("MongoDB.Client.Bson.Writer.BsonWriter");
        private static readonly MemberAccessExpressionSyntax StateDocLenMemberAccess = SimpleMemberAccess(StateToken, DocLenToken);
        private static readonly MemberAccessExpressionSyntax TypedStateDocLenMemberAccess = SimpleMemberAccess(TypedStateToken, DocLenToken);
        private static readonly MemberAccessExpressionSyntax TypedStateConsumedMemberAccess = SimpleMemberAccess(TypedStateToken, ConsumedToken);
        private static readonly MemberAccessExpressionSyntax StateConsumedMemberAccess = SimpleMemberAccess(StateToken, ConsumedToken);
        private static readonly EnumMemberDeclarationSyntax InProgressEnumState = SF.EnumMemberDeclaration("InProgress");
        private static readonly EnumMemberDeclarationSyntax EndMarkerEnumState = SF.EnumMemberDeclaration("EndMarker");
        private static readonly EnumMemberDeclarationSyntax MainLoopEnumState = SF.EnumMemberDeclaration("MainLoop");
        private static readonly EnumMemberDeclarationSyntax InitialEnumState = SF.EnumMemberDeclaration("Initial");
        private static string SerializerBaseStateTypeString => "MongoDB.Client.Bson.Serialization.SerializerStateBase";
        private static TypeSyntax SerializerStateBaseType => SF.ParseTypeName(SerializerBaseStateTypeString);
        public static SyntaxToken TrySkipLabel => Identifier("TRY_SKIP_LABEL");
        public static SyntaxToken NullableHasValueToken => Identifier("HasValue");
        public static SyntaxToken NullableValueToken => Identifier("Value");
        public static SyntaxToken CollectionAddToken => Identifier("Add");
        public static SyntaxToken ListCountToken => Identifier("Count");
        public static SyntaxToken WriteBsonToken => Identifier("WriteBson");
        public static SyntaxToken CreateMessageToken => Identifier("CreateMessage");
        public static SyntaxToken TryParseBsonToken => Identifier("TryParseBson");
        public static SyntaxToken TryContinueParseBsonToken => Identifier("TryContinueParseBson");
        public static SyntaxToken TryParsePrologueToken => Identifier("TryParsePrologue");
        public static SyntaxToken TryParseMainLoopToken => Identifier("TryParseMainLoop");
        public static SyntaxToken TryParseEpilogueToken => Identifier("TryParseEpilogue");
        public static SyntaxToken BsonReaderToken => Identifier("reader");
        public static SyntaxToken BsonWriterToken => Identifier("writer");
        public static SyntaxToken TryParseOutVarToken => Identifier("message");
        public static SyntaxToken WriterInputVarToken => Identifier("message");
        public static SyntaxToken BsonNameToken => Identifier("bsonName");
        public static SyntaxToken BsonTypeToken => Identifier("bsonType");
        public static SyntaxToken EndMarkerToken => Identifier("endMarker");
        public static SyntaxToken UnreadedToken => Identifier("unreaded");
        public static SyntaxToken DocLengthToken => Identifier("docLength");
        public static SyntaxToken InitialEnumStateToken => Identifier("Initial");
        public static SyntaxToken MainLoopEnumStateToken => Identifier("MainLoop");
        public static SyntaxToken EndMarkerEnumStateToken => Identifier("EndMarker");
        public static SyntaxToken InProgressEnumStateToken => Identifier("InProgress");
        public static SyntaxToken StatePropertyNameToken => Identifier("State");
        public static SyntaxToken StateToken => Identifier("state");
        public static SyntaxToken CollectionStateArgumentToken => Identifier("Argument");
        public static SyntaxToken TypedStateToken => Identifier("typedState");
        public static SyntaxToken DocLenToken => Identifier("DocLen");
        public static SyntaxToken ListDocLenToken => Identifier("listDocLength");
        public static SyntaxToken ConsumedToken => Identifier("Consumed");
        public static SyntaxToken CollectionToken => Identifier("Collection");
        public static SyntaxToken LocalCollectionToken => Identifier("collection");
        public static SyntaxToken SerializerBaseTypeToken => Identifier(SerializerBaseStateTypeString);
        public static SyntaxToken StartCheckpointToken => Identifier("startCheckpoint");
        public static SyntaxToken LoopCheckpointToken => Identifier("loopCheckpoint");
        public static SyntaxToken LocalConsumedToken => Identifier("localConsumed");
        public static SyntaxToken LocalDocLenToken => Identifier("docLen");
        public static SyntaxToken SmallConsumedToken => Identifier("consumed");
    }
}
