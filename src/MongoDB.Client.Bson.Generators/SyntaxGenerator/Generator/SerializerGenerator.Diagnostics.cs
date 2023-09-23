using Microsoft.CodeAnalysis;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static readonly string UnhandledExceptionError = "MONGO00";
        private static readonly string UnsupportedTypeError = "MONGO01";
        private static readonly string UnsupportedGenericTypeError = "MONGO02";
        private static readonly string NullableFieldsError = "MONGO03";
        private static readonly string SerializationMapUsingWarning = "MONGO04";
        private static readonly string GeneratingDurationInfo = "MONGO05";
        private static readonly string UnsupportedOperationType = "MONGO06";
        private static readonly string UnsupportedByteArrayReprError = "MONGO07";
        private static readonly string MatchConstructorParametersError = "MONGO08";
        private static readonly string DictionaryKeyTypeError = "MONGO09";
        private static readonly string SkipFlagsError = "MONGO10";

        public static void ReportSkipFlagsError(ISymbol sym)
        {
            var message = "Skip flags in BsonSerializableAttribute error";

            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(SkipFlagsError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportDictionaryKeyTypeError(ISymbol sym)
        {
            var message = "The dictionary only supports the string key parameter";

            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(DictionaryKeyTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportMatchConstructorParametersError(ISymbol sym)
        {
            var message = "Can't match constructor parameters";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(MatchConstructorParametersError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportUnsuportedByteArrayReprError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"{decl.Name} has an unsupported binary data representation: {type.ToString()}";

            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsupportedByteArrayReprError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportGenerationContextTreeError(string message = null)
        {
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsupportedOperationType, "Generation failed", message ?? "Generation context tree operations was failed", "SourceGenerator", DiagnosticSeverity.Error, true), null));
        }
        public static void ReportUnhandledException(Exception ex)
        {
            var st = ex.StackTrace.Replace('\n', ' ').Replace('\r', ' ');

            var message = $"Generator unhandled error - {ex.Message}{st}";

            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnhandledExceptionError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), Location.None));
        }
        public static void ReportUnhandledException(string message, ISymbol sym)
        {
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnhandledExceptionError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportNullableFieldsError(ISymbol decl)
        {
            var message = $"Field {decl.Name}: nullable fields not supported, try make {decl.Name} as property";

            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(NullableFieldsError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportUnsupportedTypeError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"{decl.Name} has an unsupported type: {type}";

            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsupportedTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportUnsupportedGenericTypeError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"Field or Property {decl.Name} has an unsupported generic type {type.Name}";

            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsupportedGenericTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportSerializerMapUsingWarning(ISymbol decl)
        {
            var message = "Undefined serializer type. Using SerializersMap";
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(SerializationMapUsingWarning, "Generation warn", message, "SourceGenerator", DiagnosticSeverity.Warning, true), decl.Locations[0]));
        }

        public static void ReportDuration(string stage, TimeSpan time)
        {
            ExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(GeneratingDurationInfo, "Generation info", stage + ": " + time.ToString(), "SourceGenerator", DiagnosticSeverity.Warning, true), Location.None));
        }
    }
}
