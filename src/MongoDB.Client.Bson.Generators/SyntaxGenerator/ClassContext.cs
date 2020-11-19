using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class MasterContext
    {
        public readonly List<ClassContext> Contexts;
        public readonly GeneratorExecutionContext GeneratorExecutionContext;
        public MasterContext(List<INamedTypeSymbol> symbols, GeneratorExecutionContext ctx)
        {
            Contexts = new List<ClassContext>();
            GeneratorExecutionContext = ctx;
            foreach (var symbol in symbols)
            {
                Contexts.Add(new ClassContext(this, symbol));
            }
        }
    }
    internal class ClassContext
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
        internal readonly MasterContext Root;
        internal readonly INamedTypeSymbol Declaration;
        internal readonly List<MemberContext> Members;
        internal readonly ImmutableArray<ITypeSymbol>? GenericArgs;
        internal readonly ImmutableArray<IParameterSymbol>? ConstructorParams;

        internal bool HavePrimaryConstructor => ConstructorParams.HasValue;
        internal bool HaveAnyConstructor => Declaration.Constructors.Any( method => !method.Parameters.IsEmpty);
        
        public bool ConstructorContains(string name)
        {
            if (ConstructorParams.HasValue)
            {
                _ = ConstructorParams.Value.First(type => type.Name.Equals(name));
                return true;
            }

            return false;
        }
        public ClassContext(MasterContext root, INamedTypeSymbol symbol)
        {
            Root = root;
            Declaration = symbol;
            Members = new List<MemberContext>();
            GenericArgs = !Declaration.TypeArguments.IsEmpty ? Declaration.TypeArguments : null;
            if (AttributeHelper.TryFindPrimaryConstructor(Declaration, out var constructor))
            {
                if (constructor!.Parameters.Length != 0)
                {
                    ConstructorParams = constructor.Parameters;
                }
            }
            foreach (var member in Declaration.GetMembers())
            {
                if ( (member.IsStatic && Declaration.TypeKind != TypeKind.Enum) || member.IsAbstract || AttributeHelper.IsIgnore(member) ||
                     (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field))
                {
                    continue;
                }
                if (member.DeclaredAccessibility != Accessibility.Public)
                {
                    if (ConstructorParams == null)
                    {
                        continue;
                    }

                    foreach (var param in ConstructorParams)
                    {
                        //TODO: Смотреть флов аргумента вместо проверки на имя
                        if (param.Name.Equals(member.Name))
                        {
                            Members.Add(new MemberContext(this, member));
                            break;
                        }
                    }
                    continue;
                }
                //TODO: допустимо только без гетера, если нет сетера проверить есть ли он в конструкторе
                if ( (member is IPropertySymbol {SetMethod: { }, GetMethod: { }, IsReadOnly: false}) ||
                     (member is IFieldSymbol {IsReadOnly: false}) )
                {
                    Members.Add(new MemberContext(this, member));
                }
            }
        }
    }
    internal class MemberContext
    {
        internal ClassContext Root { get; }
        internal readonly ISymbol NameSym;
        internal readonly ITypeSymbol TypeSym;
        internal readonly string BsonElementAlias;
        internal readonly string BsonElementValue;
        internal readonly ImmutableArray<ITypeSymbol>? TypeGenericArgs;
        internal SyntaxToken AssignedVariable;
        
        internal bool IsGenericType => Root.GenericArgs?.FirstOrDefault( sym => sym.Name.Equals(TypeSym.Name)) != default;
        public MemberContext(ClassContext root, ISymbol memberSym)
        {
            Root = root;
            NameSym = memberSym;
            
            switch (NameSym)
            {
                case IFieldSymbol field:
                    TypeSym = field.Type;
                    break;
                case IPropertySymbol prop:
                    TypeSym = prop.Type;
                    break;
                default: break;
            }

            if (TypeSym is INamedTypeSymbol namedType)
            {
                TypeGenericArgs = !namedType.TypeArguments.IsEmpty ? namedType.TypeArguments : null;
            }

            (BsonElementValue, BsonElementAlias) = AttributeHelper.GetMemberAlias(NameSym);
        }
    }
}