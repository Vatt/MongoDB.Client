using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MongoDB.Client.Bson.Generators
{
    internal static class BsonGeneratorReadOperations
    {
        internal static readonly Dictionary<string, string> SimpleOperations = new Dictionary<string, string>()
        {
            ["Double"] = @"if (!reader.TryGetDouble(out {0})) {{ return false; }}",
            ["String"] = @"if (!reader.TryGetDouble(out {0})) {{ return false; }}",
            ["BsonDocument"] = @"if (!reader.TryParseDocument(null, out {0})) {{ return false; }}",
            ["BsonObjectId"] = @"if (!reader.TryGetObjectId(out {0})) {{ return false; }}",
            ["Boolean"] = @"if (!reader.TryGetBoolean(out {0})) {{ return false; }}",
            ["Int32"] = @"if (!reader.TryGetInt32(out {0})) {{ return false; }}",
            ["Int64"] = @"if (!reader.TryGetInt64(out {0})) {{ return false; }}",

        };
        internal static readonly Dictionary<string, int[]> SupportedHardOperationsBsonTypesMap = new Dictionary<string, int[]>
        {
            ["Guid"] = new int[] { 5, 2 },
            ["DateTimeOffset"] = new int[] { 3, 9, 18 },
        };
        internal static readonly Dictionary<(string, int), string> HardOperations = new Dictionary<(string, int), string>()
        {
            [("Guid", 5)] = @"if (!reader.TryGetBinaryDataGuid(out {0})) {{ return false; }}",
            [("Guid", 2)] = @"if (!reader.TryGetGuidFromString(out {0})) {{ return false; }}",
            [("DateTimeOffset", 3)] = @"if (!reader.TryGetDatetimeFromDocument(out {0})) {{ return false; }}",
            [("DateTimeOffset", 9)] = @"if (!reader.TryGetUTCDatetime(out {0})) {{ return false; }}",
            [("DateTimeOffset", 18)] = @"if (!reader.TryGetUTCDatetime(out {0})) {{ return false; }}",
        };
        internal static readonly Dictionary<string, string> GeneratedSerializatorsOperations = new Dictionary<string, string>();
        internal static bool TryGetSimpleType(ITypeSymbol sym, out string readOp)
        {
            return SimpleOperations.TryGetValue(sym.Name, out readOp);
        }
        internal static bool IsHardType(ITypeSymbol sym)
        {
            return SupportedHardOperationsBsonTypesMap.ContainsKey(sym.Name);
        }
        internal static bool IsGeneratedType(ITypeSymbol sym)
        {
            return GeneratedSerializatorsOperations.ContainsKey(sym.Name);
        }
        internal static bool TryGetGeneratedType(ITypeSymbol sym, out string readOp)
        {
            return GeneratedSerializatorsOperations.TryGetValue(sym.Name, out readOp);
        }
        internal static bool IsSuportedType(ITypeSymbol sym)
        {
            if (SimpleOperations.ContainsKey(sym.Name)) { return true; }
            if (IsHardType(sym)) { return true; }
            if (IsGeneratedType(sym)) { return true; }
            return false;
        }
    }
}
