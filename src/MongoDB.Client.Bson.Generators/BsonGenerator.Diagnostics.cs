using Microsoft.CodeAnalysis;
namespace MongoDB.Client.Bson.Generators
{
    public partial class BsonGenerator
    {
        private const string UnhandledExceptionError = "MONGO00";
        private const string UnsupportedTypeError = "MONGO01";
        private const string UnsupportedGenericTypeError = "MONGO02";
        private const string NullableFieldsError = "MONGO03";
        private const string SerializationMapUsingWarning = "MONGO04";
        private const string GeneratingDurationInfo = "MONGO05";
        private const string UnsupportedOperationType = "MONGO06";
        private const string UnsupportedByteArrayReprError = "MONGO07";
        private const string MatchConstructorParametersError = "MONGO08";
        private const string DictionaryKeyTypeError = "MONGO09";
        private const string SkipFlagsError = "MONGO10";

        public static void ReportSkipFlagsError(ISymbol sym)
        {
            var message = "Skip flags in BsonSerializableAttribute error";

            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(SkipFlagsError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportDictionaryKeyTypeError(ISymbol sym)
        {
            var message = "The dictionary only supports the string key parameter";

            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(DictionaryKeyTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportMatchConstructorParametersError(ISymbol sym)
        {
            var message = "Can't match constructor parameters";
            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(MatchConstructorParametersError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportUnsuportedByteArrayReprError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"{decl.Name} has an unsupported binary data representation: {type.ToString()}";

            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsupportedByteArrayReprError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportGenerationContextTreeError(string message = null)
        {
            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsupportedOperationType, "Generation failed", message ?? "Generation context tree operations was failed", "SourceGenerator", DiagnosticSeverity.Error, true), null));
        }
        public static void ReportUnhandledException(Exception ex)
        {
            var st = ex.StackTrace.Replace('\n', ' ').Replace('\r', ' ');

            var message = $"Generator unhandled error - {ex.Message}{st}";

            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnhandledExceptionError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), Location.None));
        }
        public static void ReportUnhandledException(string message, ISymbol sym)
        {
            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnhandledExceptionError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), sym.Locations[0]));
        }
        public static void ReportNullableFieldsError(ISymbol decl)
        {
            var message = $"Field {decl.Name}: nullable fields not supported, try make {decl.Name} as property";

            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(NullableFieldsError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportUnsupportedTypeError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"{decl.Name} has an unsupported type: {type}";

            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsupportedTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportUnsupportedGenericTypeError(ISymbol decl, ITypeSymbol type)
        {
            var message = $"Field or Property {decl.Name} has an unsupported generic type {type.Name}";

            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(UnsupportedGenericTypeError, "Generation failed", message, "SourceGenerator", DiagnosticSeverity.Error, true), decl.Locations[0]));
        }
        public static void ReportSerializerMapUsingWarning(ISymbol decl)
        {
            var message = "Undefined serializer type. Using SerializersMap";
            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(SerializationMapUsingWarning, "Generation warn", message, "SourceGenerator", DiagnosticSeverity.Warning, true), decl.Locations[0]));
        }

        public static void ReportDuration(string stage, TimeSpan time)
        {
            Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(GeneratingDurationInfo, "Generation info", stage + ": " + time.ToString(), "SourceGenerator", DiagnosticSeverity.Warning, true), Location.None));
        }
    }
}
