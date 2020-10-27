using Microsoft.CodeAnalysis;
using System;
using System.Text;

namespace MongoDB.Client.Bson.Generators
{
    partial class BsonSerializatorGenerator
    {
        //private string GenerateReadArray(ClassDeclMeta classdecl, MemberDeclarationMeta memberdecl)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    builder.Append($@"
        //            result.{memberdecl.DeclSymbol.Name} = new List<{memberdecl.GenericType.Name}>();
        //            if (!reader.TryGetInt32(out var arrayDocLength)) {{ return false; }}
        //            var arrayUnreaded = reader.Remaining + sizeof(int);
        //            while (arrayUnreaded - reader.Remaining < arrayDocLength - 1)
        //            {{
        //                if ( !reader.TryGetCStringAsSpan(out var index)) {{ return false; }}
        //                {GenerateArrayReadOp(memberdecl, "var", "arrayElement")}
        //            }}
        //            if ( !reader.TryGetByte(out var arrayEndMarker)){{ return false; }}
        //            if (arrayEndMarker != '\x00')
        //            {{
        //                throw new ArgumentException($""{classdecl.ClassSymbol.Name}GeneratedSerializer.TryParse End document marker missmatch"");
        //            }}             
        //");
        //    return builder.ToString();
        //}
    }
}
