using Microsoft.CodeAnalysis;

namespace MongoDB.Client.Bson.Generators
{
    partial class BsonSerializatorGenerator
    {
        private void ReportNullableFieldMaybe(MemberDeclarationInfo declinfo)
        {
            if (declinfo.DeclType.Name.Equals("Nullable") && !declinfo.IsProperty)
            {
                BsonGeneratorErrorHelper.ReportNullableFieldsError(_context, declinfo.DeclSymbol, declinfo.DeclType, declinfo.DeclSymbol.Locations[0]);
            }
        }
        private void ReportUnsuportedTypeMaybe(MemberDeclarationInfo declinfo)
        {
            if (!BsonGeneratorReadOperations.IsSuportedType(declinfo.DeclType))
            {
                BsonGeneratorErrorHelper.ReportUnsuporterTypeError(_context, declinfo.DeclSymbol, declinfo.DeclType, declinfo.DeclSymbol.Locations[0]);
            }
        }
        private void ReportUnsuportedGenericTypeMaybe(MemberDeclarationInfo declinfo, ITypeSymbol genericSym)
        {
            if (!BsonGeneratorReadOperations.IsSuportedType(genericSym))
            {
                BsonGeneratorErrorHelper.ReportUnsuporterTypeError(_context, declinfo.DeclSymbol, declinfo.DeclType, declinfo.DeclSymbol.Locations[0]);
            }

        }
    }
}
