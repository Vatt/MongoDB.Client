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
        private static readonly string SerializerBaseStateTypeString = "MongoDB.Client.Bson.Serialization.SerializerStateBase";
        private static readonly TypeSyntax SerializerStateBaseType = SF.ParseTypeName(SerializerBaseStateTypeString);
        public static readonly SyntaxToken TrySkipLabel = Identifier("TRY_SKIP_LABEL");
        public static readonly SyntaxToken NullableHasValueToken = Identifier("HasValue");
        public static readonly SyntaxToken NullableValueToken = Identifier("Value");
        public static readonly SyntaxToken CollectionAddToken = Identifier("Add");
        public static readonly SyntaxToken ListCountToken = Identifier("Count");
        public static readonly SyntaxToken WriteBsonToken = Identifier("WriteBson");
        public static readonly SyntaxToken CreateMessageToken = Identifier("CreateMessage");
        public static readonly SyntaxToken TryParseBsonToken = Identifier("TryParseBson");
        public static readonly SyntaxToken BsonReaderToken = Identifier("reader");
        public static readonly SyntaxToken BsonWriterToken = Identifier("writer");
        public static readonly SyntaxToken TryParseOutVarToken = Identifier("message");
        public static readonly SyntaxToken WriterInputVarToken = Identifier("message");
        public static readonly SyntaxToken BsonNameToken = Identifier("bsonName");
        public static readonly SyntaxToken BsonTypeToken = Identifier("bsonType");
        public static readonly SyntaxToken EndMarkerToken = Identifier("endMarker");
        public static readonly SyntaxToken UnreadedToken = Identifier("unreaded");
        public static readonly SyntaxToken DocLengthToken = Identifier("docLength");
        private static readonly SyntaxToken InitialEnumStateToken = Identifier("Initial");
        private static readonly SyntaxToken MainLoopEnumStateToken = Identifier("MainLoop");
        private static readonly SyntaxToken EndMarkerEnumStateToken = Identifier("EndMarker");
        private static readonly SyntaxToken InProgressEnumStateToken = Identifier("InProgress");
        private static readonly SyntaxToken StatePropertyNameToken = Identifier("State");
        private static readonly SyntaxToken StateToken = Identifier("state");
        private static readonly SyntaxToken TypedStateToken = Identifier("typedState");
        private static readonly SyntaxToken DocLenToken = Identifier("DocLen");
        private static readonly SyntaxToken ConsumedToken = Identifier("Consumed");
        private static readonly SyntaxToken CollectionToken = Identifier("Collection");
        private static readonly SyntaxToken SerializerBaseTypeToken = Identifier(SerializerBaseStateTypeString);
        public static readonly SyntaxToken StartCheckpointToken = Identifier("startCheckpoint");
        public static readonly SyntaxToken LoopCheckpointToken = Identifier("loopCheckpoint");
        public static readonly SyntaxToken LocalConsumedToken = Identifier("localConsumed");
    }
}
