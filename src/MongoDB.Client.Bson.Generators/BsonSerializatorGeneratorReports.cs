using Microsoft.CodeAnalysis;

namespace MongoDB.Client.Bson.Generators
{
    internal class DiagnosticReports 
    {
        private static void ReportNullableFieldMaybe(MemberDeclarationMeta declinfo)
        {
            //if (declinfo.DeclType.Name.Equals("Nullable") && !declinfo.IsProperty)
            //{
            //    BsonGeneratorErrorHelper.ReportNullableFieldsError(_context, declinfo.DeclSymbol, declinfo.DeclType, declinfo.DeclSymbol.Locations[0]);
            //}
        }
        private static void ReportUnsuportedTypeMaybe(MemberDeclarationMeta declinfo)
        {
            if (!BsonGeneratorReadOperations.IsSuportedType(declinfo.DeclType))
            {
                //BsonGeneratorErrorHelper.ReportUnsuporterTypeError(_context, declinfo.DeclSymbol, declinfo.DeclType, declinfo.DeclSymbol.Locations[0]);
            }
        }
        private static void ReportUnsuportedGenericTypeMaybe(MemberDeclarationMeta declinfo, ITypeSymbol genericSym)
        {
            if (!BsonGeneratorReadOperations.IsSuportedType(genericSym))
            {
                //BsonGeneratorErrorHelper.ReportUnsuporterTypeError(_context, declinfo.DeclSymbol, declinfo.DeclType, declinfo.DeclSymbol.Locations[0]);
            }

        }
    }
}
