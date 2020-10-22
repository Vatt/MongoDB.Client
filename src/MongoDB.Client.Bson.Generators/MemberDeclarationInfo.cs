using Microsoft.CodeAnalysis;
//using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Bson.Generators
{
    internal class MemberDeclarationInfo
    {

        public ISymbol DeclSymbol { get; }
        public INamedTypeSymbol DeclType { get; }
        public string StringFieldNameAlias { get; }
        public bool IsProperty => DeclSymbol.Kind == SymbolKind.Property;
        public bool IsGenericList => (DeclType.ToString().Contains("System.Collections.Generic.List") || DeclType.ToString().Contains("System.Collections.Generic.IList")) && DeclType.IsGenericType;
        public bool IsClassOrStruct => DeclType.TypeKind == TypeKind.Class || DeclType.TypeKind == TypeKind.Struct;
        public ITypeSymbol GenericType => DeclType.TypeArguments[0];
        public MemberDeclarationInfo(ISymbol symbol)
        {
            DeclSymbol = symbol;
            if (DeclSymbol is IFieldSymbol fieldSym)
            {
                DeclType = fieldSym.Type as INamedTypeSymbol;
            }
            if (DeclSymbol is IPropertySymbol propSym)
            {
                DeclType = propSym.Type as INamedTypeSymbol;
            }


            if (TryGetElementNameFromBsonAttribute(out string attrName))
            {
                attrName = attrName.Replace(' ', '_');
                attrName = attrName.Replace('(', '_');
                attrName = attrName.Replace(')', '_');
                StringFieldNameAlias = attrName;
            }
            else
            {
                StringFieldNameAlias = DeclSymbol.Name;
            }
        }
        public bool TryGetOneGenericArgument(out INamedTypeSymbol genericType)
        {
            genericType = default;
            if (DeclType.TypeArguments == null || DeclType.TypeArguments.Length == 0)
            {
                return false;
            }
            genericType = DeclType.TypeArguments[0] as INamedTypeSymbol;
            return true;
        }
        public bool TryGetBsonAttribute(out AttributeData attrData)
        {
            attrData = default;
            if (!(DeclSymbol.GetAttributes().Length > 0))
            {
                return false;
            }
            foreach (var attr in DeclSymbol.GetAttributes())
            {
                if (attr.AttributeClass.Name.Equals("BsonElementField"))
                {
                    attrData = attr;
                    return true;
                }
            }
            return false;
        }
        public bool TryGetElementNameFromBsonAttribute(out string name)
        {
            name = default;
            if (!TryGetBsonAttribute(out var attrData))
            {
                return false;
            }
            name = attrData.NamedArguments[0].Value.Value.ToString(); //TODO: FIX IT
            return true;

        }

    }
}
