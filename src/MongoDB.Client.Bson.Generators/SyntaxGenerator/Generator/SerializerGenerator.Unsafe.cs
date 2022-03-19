using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly SyntaxToken UnsafeToken = Identifier("System.Runtime.CompilerServices.Unsafe");
        public static readonly SyntaxToken MemoryMarshalToken = Identifier("System.Runtime.InteropServices.MemoryMarshal");
        public static readonly SyntaxToken UnsafeAddToken = Identifier("Add");
        public static ExpressionSyntax UnsafeAddExpr(ExpressionSyntax expr, ExpressionSyntax elementOffsetExpr)
        {
            return InvocationExpr(UnsafeToken, UnsafeAddToken, RefArgument(elementOffsetExpr));
        }

        public static ExpressionSyntax MemoryMarshalGetArrayDataReference(SyntaxToken arrayToken)
        {
            return InvocationExpr(MemoryMarshalToken, GenericName(Identifier("GetArrayDataReference"), BytePredefinedType()), Argument(arrayToken));
        }
        public static ExpressionSyntax MemoryMarshalGetReference(SyntaxToken spanToken)
        {
            return InvocationExpr(MemoryMarshalToken, Identifier("GetReference"), Argument(spanToken));
        }

        public static ExpressionSyntax GetSpanElementUnsafe(SyntaxToken span, int index)
        {
            if (index == 0)
            {
                return MemoryMarshalGetReference(span);
            }
            return InvocationExpr(
                UnsafeToken,
                UnsafeAddToken,
                RefArgument(MemoryMarshalGetReference(span)),
                Argument(CastToNInt(NumericLiteralExpr(index))));
        }
    }
}
