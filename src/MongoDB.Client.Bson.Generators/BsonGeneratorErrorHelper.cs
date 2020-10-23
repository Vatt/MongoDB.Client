using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDB.Client.Bson.Generators
{
    #nullable enable
    static class BsonGeneratorErrorHelper
    {
        private static readonly string UnsuportedTypeErrorCode = "MONGOGEN01";
        private static readonly string UnsuportedGenericTypeErrorCode = "MONGOGEN02";
        private static readonly string NullableFieldsErrorCode = "MONGOGEN03";
        public static void ReportNullableFieldsError(GeneratorExecutionContext context, ISymbol decl, ITypeSymbol type, Location? location)
        {
            var message = $"Field {decl.Name}: nullable fields not suported, try make {decl.Name} as property";
            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(NullableFieldsErrorCode, "Generation failed", message, "Generation", DiagnosticSeverity.Error, true), location));
        }
        public static void ReportUnsuporterTypeError(GeneratorExecutionContext context, ISymbol decl, ITypeSymbol type, Location? location)
        {
            var message = $"Field or Property {decl.Name} has an unsuported type {type.Name}";
            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsuportedTypeErrorCode, "Generation failed", message, "Generation", DiagnosticSeverity.Error, true), location));
        }
        public static void ReportUnsuporterGenericTypeError(GeneratorExecutionContext context, ISymbol decl, ITypeSymbol type, Location? location)
        {
            var message = $"Field or Property {decl.Name} has an unsuported generic type {type.Name}";
            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsuportedGenericTypeErrorCode, "Generation failed", message, "Generation", DiagnosticSeverity.Error, true), location));
        }
    }
}
