using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    public static class Types
    {
        public static Compilation Compilation;
        public static bool TryGetMetadata(ITypeSymbol source, out ISymbol result)
        {
            result = default;
            var str = source.ToString();
            result = Compilation.GetTypeByMetadataName(str);
            if (result != null)
            {
                return true;
            }

            while (true)
            {
                var last = str.LastIndexOf('.');
                if (last == -1)
                {
                    break;
                }
                StringBuilder builder = new StringBuilder(str);
                str = builder.Replace('.', '+', last, 1).ToString();

                result = Compilation.GetTypeByMetadataName(str);
                if (result != null)
                {
                    return true;
                }

            }         
            return false;
        }
        public static void TypesInit(Compilation compilation)
        {
            Compilation = compilation;
            ListSymbol = Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
            IListSymbol = Compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1");
        }
        public static ISymbol ListSymbol; 
        public static ISymbol IListSymbol; 
    }
}