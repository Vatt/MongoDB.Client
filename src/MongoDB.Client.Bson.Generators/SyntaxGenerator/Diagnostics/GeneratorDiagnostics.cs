using Microsoft.CodeAnalysis;
using System;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics
{

    static class GeneratorDiagnostics
    {
        private static GeneratorExecutionContext _ctx => BsonSerializerGenerator.Context;
        private static readonly string UnhandledExceptionError = "MONGO00";
        private static readonly string UnsuportedTypeError = "MONGO01";
        private static readonly string UnsuportedGenericTypeError = "MONGO02";
        private static readonly string NullableFieldsError = "MONGO03";
        private static readonly string SerializationMapUsingWarning = "MONGO04";
        private static readonly string GeneratingDurationInfo = "MONGO05";
        //public static void Init(GeneratorExecutionContext ctx)
        //{
        //    _ctx = ctx;
        //}
        public static void ReportUnhandledException(Exception ex)
        {
            var st = ex.StackTrace.Replace('\n', ' ').Replace('\r', ' ');
            var message = $"Generator unhandled error - {ex.Message}{st}";
            _ctx.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnhandledExceptionError, "Generation failed",
                message, "SourceGenerator", DiagnosticSeverity.Error, true), Location.None));
        }
        public static void ReportUnhandledException(string message, ISymbol sym)
        {
            _ctx.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnhandledExceptionError, "Generation failed",
                message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportNullableFieldsError(ISymbol decl)
        {
            var message = $"Field {decl.Name}: nullable fields not suported, try make {decl.Name} as property";
            _ctx.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(NullableFieldsError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportUnsuporterTypeError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"{decl.Name} has an unsupported type: {type.ToString()}";
            _ctx.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsuportedTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportUnsuporterGenericTypeError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"Field or Property {decl.Name} has an unsuported generic type {type.Name}";
            _ctx.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsuportedGenericTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportSerializerMapUsingWarning(ISymbol decl)
        {
            var message = "Undefined serializer type. Using SerializersMap";
            _ctx.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(SerializationMapUsingWarning, "Generation warn", message, "SourceGenerator", DiagnosticSeverity.Warning, true), decl.Locations[0]));
        }

        public static void ReportDuration(string stage, TimeSpan time)
        {
            _ctx.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(GeneratingDurationInfo, "Generation info",
               stage + ": " + time.ToString(), "SourceGenerator", DiagnosticSeverity.Warning, true), Location.None));
        }
    }
}
