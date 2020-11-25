using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{

    internal class ContextCore
    {
        internal SyntaxToken BsonReaderToken => SF.Identifier("reader");
        internal IdentifierNameSyntax BsonReaderId => SF.IdentifierName(BsonReaderToken);
        internal TypeSyntax BsonReaderType => SF.ParseTypeName("MongoDB.Client.Bson.Reader.BsonReader");
        internal SyntaxToken BsonWriterToken => SF.Identifier("writer");
        internal IdentifierNameSyntax BsonWriterId => SF.IdentifierName(BsonWriterToken);
        internal TypeSyntax BsonWriterType => SF.ParseTypeName("MongoDB.Client.Bson.Writer.BsonWriter");
        internal SyntaxToken TryParseOutVarToken => SF.Identifier("message");
        internal IdentifierNameSyntax TryParseOutVar => SF.IdentifierName(TryParseOutVarToken);
        internal IdentifierNameSyntax WriterInputVar => SF.IdentifierName("message");
        internal SyntaxToken WriterInputVarToken => SF.Identifier("message");

        internal bool IsRecord;
        internal MasterContext Root;
        internal TypeLib Types;
        internal INamedTypeSymbol Declaration;
        internal SyntaxNode DeclarationNode;
        internal List<MemberContext> Members;
        internal ImmutableArray<ITypeSymbol>? GenericArgs;
        internal ImmutableArray<IParameterSymbol>? ConstructorParams;

        internal bool HavePrimaryConstructor => ConstructorParams.HasValue;

        public bool ConstructorContains(string name)
        {
            if (ConstructorParams.HasValue)
            {
                _ = ConstructorParams.Value.First(type => type.Name.Equals(name));
                return true;
            }

            return false;
        }       
    }    
}