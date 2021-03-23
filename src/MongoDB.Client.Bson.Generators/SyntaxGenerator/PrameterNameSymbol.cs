using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
#pragma warning disable RS1009 // Only internal implementations of this interface are allowed.
    internal class ParameterNameSymbol : IParameterSymbol
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed.
    {
        private readonly IParameterSymbol _baseSym;
        public ParameterNameSymbol(IParameterSymbol baseSym)
        {
            _baseSym = baseSym;
        }
        public SymbolKind Kind => _baseSym.Kind;

        public string Language => "C#";

        public string Name => _baseSym.Name;
        public ITypeSymbol Type => _baseSym.Type;
        public string MetadataName => _baseSym.MetadataName;

        public ISymbol ContainingSymbol => _baseSym.ContainingSymbol;

        public IAssemblySymbol ContainingAssembly => _baseSym.ContainingAssembly;

        public IModuleSymbol ContainingModule => _baseSym.ContainingModule;

        public INamedTypeSymbol ContainingType => _baseSym.ContainingType;

        public INamespaceSymbol ContainingNamespace => _baseSym.ContainingNamespace;

        public bool IsDefinition => false;

        public bool IsStatic => false;

        public bool IsVirtual => false;

        public bool IsOverride => false;

        public bool IsAbstract => false;

        public bool IsSealed => false;

        public bool IsExtern => false;

        public bool IsImplicitlyDeclared => false;

        public bool CanBeReferencedByName => false;

        public ImmutableArray<Location> Locations => _baseSym.Locations;

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _baseSym.DeclaringSyntaxReferences;

        public Accessibility DeclaredAccessibility => _baseSym.DeclaredAccessibility;


        public bool HasUnsupportedMetadata => _baseSym.HasUnsupportedMetadata;

        public RefKind RefKind => _baseSym.RefKind;

        public bool IsParams => _baseSym.IsParams;

        public bool IsOptional => _baseSym.IsOptional;

        public bool IsThis => _baseSym.IsThis;

        public bool IsDiscard => _baseSym.IsDiscard;

        public NullableAnnotation NullableAnnotation => _baseSym.NullableAnnotation;

        public ImmutableArray<CustomModifier> CustomModifiers => _baseSym.CustomModifiers;

        public ImmutableArray<CustomModifier> RefCustomModifiers => _baseSym.RefCustomModifiers;

        public int Ordinal => _baseSym.Ordinal;

        public bool HasExplicitDefaultValue => _baseSym.HasExplicitDefaultValue;

        public object ExplicitDefaultValue => _baseSym.ExplicitDefaultValue;

        IParameterSymbol IParameterSymbol.OriginalDefinition =>  _baseSym.OriginalDefinition;

        ISymbol ISymbol.OriginalDefinition => _baseSym.OriginalDefinition;

        public void Accept(SymbolVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public TResult? Accept<TResult>(SymbolVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }

        public bool Equals(ISymbol other, SymbolEqualityComparer equalityComparer)
        {
            throw new NotImplementedException();
        }

        public bool Equals(ISymbol other)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<AttributeData> GetAttributes()
        {
            return ImmutableArray<AttributeData>.Empty;
        }

        public string GetDocumentationCommentId()
        {
            throw new NotImplementedException();
        }

        public string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null)
        {
            throw new NotImplementedException();
        }

        public string ToDisplayString(SymbolDisplayFormat format = null)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            throw new NotImplementedException();
        }

        public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            throw new NotImplementedException();
        }
    }
}
