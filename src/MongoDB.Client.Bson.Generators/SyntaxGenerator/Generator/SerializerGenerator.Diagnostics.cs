using Microsoft.CodeAnalysis;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static readonly string UnhandledExceptionError = "MONGO00";
        private static readonly string UnsuportedTypeError = "MONGO01";
        private static readonly string UnsuportedGenericTypeError = "MONGO02";
        private static readonly string NullableFieldsError = "MONGO03";
        private static readonly string SerializationMapUsingWarning = "MONGO04";
        private static readonly string GeneratingDurationInfo = "MONGO05";
        private static readonly string UnsuportedOperationType = "MONGO06";
        private static readonly string UnsuportedByteArrayReprError = "MONGO07";
        private static readonly string MatchConstructorParametersError = "MONGO08";
        private static readonly string DictionaryKeyTypeError = "MONGO09";
        //public static void Init(GeneratorExecutionContext ctx)
        //{
        //    _ctx = ctx;
        //}
        public static void ReportDictionaryKeyTypeError(ISymbol sym)
        {
            var message = "The dictionary only supports the string key parameter";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(DictionaryKeyTypeError, "Generation failed",
                message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportMatchConstructorParametersError(ISymbol sym)
        {
            var message = "Can't match constructor parameters";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(MatchConstructorParametersError, "Generation failed",
                message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportUnsuportedByteArrayReprError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"{decl.Name} has an unsupported binary data representation: {type.ToString()}";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsuportedByteArrayReprError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportGenerationContextTreeError(string message = null)
        {
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsuportedOperationType, "Generation failed",
                message ?? "Generation context tree operations was failed", "SourceGenerator", DiagnosticSeverity.Error, true), null));
        }
        public static void ReportUnhandledException(Exception ex)
        {
            var st = ex.StackTrace.Replace('\n', ' ').Replace('\r', ' ');
            var message = $"Generator unhandled error - {ex.Message}{st}";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnhandledExceptionError, "Generation failed",
                message, "SourceGenerator", DiagnosticSeverity.Error, true), Location.None));
        }
        public static void ReportUnhandledException(string message, ISymbol sym)
        {
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnhandledExceptionError, "Generation failed",
                message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportNullableFieldsError(ISymbol decl)
        {
            var message = $"Field {decl.Name}: nullable fields not suported, try make {decl.Name} as property";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(NullableFieldsError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportUnsuporterTypeError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"{decl.Name} has an unsupported type: {type.ToString()}";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsuportedTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportUnsuporterGenericTypeError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"Field or Property {decl.Name} has an unsuported generic type {type.Name}";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsuportedGenericTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportSerializerMapUsingWarning(ISymbol decl)
        {
            var message = "Undefined serializer type. Using SerializersMap";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(SerializationMapUsingWarning, "Generation warn", message, "SourceGenerator", DiagnosticSeverity.Warning, true), decl.Locations[0]));
        }

        public static void ReportDuration(string stage, TimeSpan time)
        {
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(GeneratingDurationInfo, "Generation info",
               stage + ": " + time.ToString(), "SourceGenerator", DiagnosticSeverity.Warning, true), Location.None));
        }
    }
}
