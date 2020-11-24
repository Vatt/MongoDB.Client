﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class MasterContext
    {
        public readonly List<ClassContext> Contexts;
        public readonly Compilation Compilation;
        public MasterContext(List<INamedTypeSymbol> symbols, GeneratorExecutionContext ctx)
        {
            Contexts = new List<ClassContext>();
            Compilation = ctx.Compilation;
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
        internal bool HaveAnyConstructor => Declaration.Constructors.Any(method => !method.Parameters.IsEmpty);

        public bool ConstructorContains(string name)
        {
            if (ConstructorParams.HasValue)
            {
                _ = ConstructorParams.Value.First(type => type.Name.Equals(name));
                return true;
            }

            return false;
        }
        private void ProcessBaseList(INamedTypeSymbol symbol)
        {
            if (symbol == null)
            {
                return;
            }
            foreach (var member in symbol.GetMembers())
            {
                if ((member.IsStatic && Declaration.TypeKind != TypeKind.Enum) || member.IsAbstract || AttributeHelper.IsIgnore(member) ||
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
                if ((member is IPropertySymbol { SetMethod: { }, GetMethod: { }, IsReadOnly: false }) ||
                     (member is IFieldSymbol { IsReadOnly: false }))
                {
                    Members.Add(new MemberContext(this, member));
                }
                
            }
           // ProcessBaseList(symbol.BaseType);
        }
        public ClassContext(MasterContext root, INamedTypeSymbol symbol)
        {
            Root = root;
            Declaration = symbol;
            Members = new List<MemberContext>();
            GenericArgs = Declaration.TypeArguments.IsEmpty ? null : Declaration.TypeArguments;
            if (AttributeHelper.TryFindPrimaryConstructor(Declaration, out var constructor))
            {
                if (constructor!.Parameters.Length != 0)
                {
                    ConstructorParams = constructor.Parameters;
                }
            }
            else
            {
                if (Declaration.GetMembers().Any(x => x.Kind == SymbolKind.Property && x.Name == "EqualityContract" && x.IsImplicitlyDeclared)) // record shit check
                {
                    if (Declaration.Constructors.Length == 2)
                    {
                        ConstructorParams = Declaration.Constructors.Where(x => x.Parameters[0].Type != Declaration).First().Parameters;
                    }
                    
                }

            }
            foreach (var member in Declaration.GetMembers())
            {
                if ((member.IsStatic && Declaration.TypeKind != TypeKind.Enum) || member.IsAbstract || AttributeHelper.IsIgnore(member) ||
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
                if ((member is IPropertySymbol { SetMethod: { }, GetMethod: { }, IsReadOnly: false }) ||
                     (member is IFieldSymbol { IsReadOnly: false }))
                {
                    Members.Add(new MemberContext(this, member));
                }                
            }
            ProcessBaseList(symbol.BaseType);

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
        internal readonly ISymbol TypeMetadata;
        internal bool IsGenericType => Root.GenericArgs?.FirstOrDefault(sym => sym.Name.Equals(TypeSym.Name)) != default;
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
            if (TypeSym != null)
            {
                _ = Types.TryGetMetadata(TypeSym, out TypeMetadata);
            }
             
            var some = Root.Root.Compilation.GetTypesByMetadataName(TypeSym!.ToString()).ToArray();
            if (TypeSym is INamedTypeSymbol namedType)
            {
                TypeGenericArgs = namedType.TypeArguments.IsEmpty ? null : namedType.TypeArguments;
            }

            (BsonElementValue, BsonElementAlias) = AttributeHelper.GetMemberAlias(NameSym);
        }

    }
}