using Microsoft.CodeAnalysis;
using System;
using System.Text;

namespace MongoDB.Client.Bson.Generators
{
    partial class BsonSerializatorGenerator
    {
        private string GenerateReaderMethod(ClassDeclMeta info)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@"
            bool IGenericBsonSerializer<{info.ClassSymbol.Name}>.TryParse(ref MongoDBBsonReader reader, out {info.ClassSymbol.Name} message)
            {{
                message = default;
                var result = new {info.ClassSymbol.Name}();
                if (!reader.TryGetInt32(out var docLength)) {{ return false; }}
                var unreaded = reader.Remaining + sizeof(int);
                while (unreaded - reader.Remaining < docLength - 1)
                {{
                    if (!reader.TryGetByte(out var bsonType)) {{ return false; }}
                    if (!reader.TryGetCStringAsSpan(out var bsonName)) {{ return false; }}                                               
                    {GenerateReads(info)}

                    throw new ArgumentException($""{info.ClassSymbol.Name}.TryParse  with bson type number {{bsonType}}"");      
                }}
                if ( !reader.TryGetByte(out var endMarker)){{ return false; }}
                if (endMarker != '\x00'){{ throw new ArgumentException(""{info.ClassSymbol.Name}GeneratedSerializator.TryParse End document marker missmatch""); }}

                message = result;
                return true;
            }}

        ");
            return builder.ToString();
        }
        private string GenerateReads(ClassDeclMeta info)
        {
            StringBuilder buidler = new StringBuilder();
            foreach (var declinfo in info.MemberDeclarations)
            {
                buidler.Append($@"
                    if (bsonName.SequenceEqual( {info.ClassSymbol.Name}{declinfo.StringFieldNameAlias}))
                    {{
                        if( bsonType == 10 )
                        {{
                            result.{declinfo.DeclSymbol.Name} = default;
                            continue;
                        }}
                        {GenerateReadsAndAssign(info, declinfo)}
                        continue;
                    }}
                ");
            }
            return buidler.ToString();
        }

        private string GenerateReadsAndAssign(ClassDeclMeta info, MemberDeclarationMeta declinfo)
        {
            StringBuilder builder = new StringBuilder();
            ReportNullableFieldMaybe(declinfo);
            ITypeSymbol type = declinfo.DeclType;
            if (declinfo.DeclType.Name.Equals("Nullable"))
            {
                type = declinfo.DeclType.TypeArguments[0];
            }

            if (BsonGeneratorReadOperations.TryGetSimpleType(type, out var readOp))
            {
                return GenerateSimpleReadOpAndAssign(declinfo, readOp, "var", "value", $"result.{declinfo.DeclSymbol.Name} = value;");

            }
            if (BsonGeneratorReadOperations.IsHardType(type))
            {
                return GeneratehHardReadOpAndAssign(declinfo, type, "var", "value", $"result.{declinfo.DeclSymbol.Name} = value;");
            }
            switch (type.Name)
            {
                default:
                    {
                        if (declinfo.IsGenericList && !declinfo.GenericType.Name.Equals("BsonDocument"))
                        {
                            ReportUnsuportedGenericTypeMaybe(declinfo, declinfo.GenericType);

                            builder.Append(GenerateReadArray(info, declinfo));
                            break;
                        }
                        if (declinfo.IsGenericList && declinfo.GenericType.Name.Equals("BsonDocument"))
                        {
                            builder.Append(GenerateReadArrayToBsonDocumentList(info, declinfo));
                            break;
                        }
                        ReportUnsuportedTypeMaybe(declinfo);
                        if (declinfo.IsClassOrStruct)
                        {
                            builder.Append(GenerateReadOtherDocument(info, declinfo));
                            break;
                        }

                        return string.Empty;
                        //throw new ArgumentException($"{nameof(BsonSerializatorGenerator)}.{nameof(GenerateReadsAndAssign)} with type name {declinfo.DeclType.Name}");
                    }


            }
            return builder.ToString();
        }

        private string GenerateSimpleReadOpAndAssign(MemberDeclarationMeta declinfo, string readOp, string typeArg, string varArg, string assignOp, bool forceAssign = false)
        {
            var builder = new StringBuilder();
            if (forceAssign)
            {
                builder.Append($@"
                        {String.Format(readOp, $"{typeArg} {varArg}")}
                        {assignOp}
                        ");

                return builder.ToString();
            }


            if (declinfo.IsProperty)
            {
                builder.Append($@"
                        {String.Format(readOp, $"{typeArg} {varArg}")}
                        {assignOp}
                        ");

                return builder.ToString();
            }
            else
            {
                if (forceAssign)
                {
                    builder.Append($@"
                        {String.Format(readOp, $"{typeArg} {varArg}")}
                        {assignOp}
                        ");
                }
                else
                {
                    builder.Append($@"
                        {String.Format(readOp, $"result.{declinfo.DeclSymbol.Name}")}
                        ");
                }
            }
            return builder.ToString();
        }

        private string GeneratehHardReadOpAndAssign(MemberDeclarationMeta declinfo, ITypeSymbol sym, string typeArg, string varArg, string assignOp)
        {
            var builder = new StringBuilder();
            foreach (var bsonType in BsonGeneratorReadOperations.SupportedHardOperationsBsonTypesMap[sym.Name])
            {
                var readOp = BsonGeneratorReadOperations.HardOperations[(sym.Name, bsonType)];
                builder.Append($@"
                        if( bsonType == {bsonType} )
                        {{
                                {GenerateSimpleReadOpAndAssign(declinfo, readOp, typeArg, varArg, assignOp)}
                                continue;
                        }}");
            }
            builder.Append($@"
                        throw new ArgumentException(""{declinfo.DeclSymbol.Name} unsupported DateTimeOffset type"");
            ");
            return builder.ToString();
        }

        private string GenerateReadArrayToBsonDocumentList(ClassDeclMeta classdecl, MemberDeclarationMeta memberdecl)
        {
            StringBuilder builder = new StringBuilder();
            if (memberdecl.IsProperty)
            {
                builder.Append($@"                    
                     if ( !reader.TryGetArrayAsDocumentList(out var docList)){{ return false;}}            
                     result.{memberdecl.DeclSymbol.Name} = docList;

            ");
            }
            else
            {
                builder.Append($@"                    
                     if ( !reader.TryGetArrayAsDocumentList(out result.{memberdecl.DeclSymbol.Name})){{ return false;}}            
            ");
            }

            return builder.ToString();
        }
        private string GenerateReadArray(ClassDeclMeta classdecl, MemberDeclarationMeta memberdecl)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@"
                    result.{memberdecl.DeclSymbol.Name} = new List<{memberdecl.GenericType.Name}>();
                    if (!reader.TryGetInt32(out var arrayDocLength)) {{ return false; }}
                    var arrayUnreaded = reader.Remaining + sizeof(int);
                    while (arrayUnreaded - reader.Remaining < arrayDocLength - 1)
                    {{
                        if ( !reader.TryGetCStringAsSpan(out var index)) {{ return false; }}
                        {GenerateArrayReadOp(memberdecl, "var", "arrayElement")}
                    }}
                    if ( !reader.TryGetByte(out var arrayEndMarker)){{ return false; }}
                    if (arrayEndMarker != '\x00')
                    {{
                        throw new ArgumentException($""{classdecl.ClassSymbol.Name}GeneratedSerializer.TryParse End document marker missmatch"");
                    }}             
        ");
            return builder.ToString();
        }
        private string GenerateArrayReadOp(MemberDeclarationMeta memberdecl, string typeArg, string varArg)
        {
            var assignOp = $"result.{memberdecl.DeclSymbol.Name}.Add({varArg});";
            var assignGenericOp = $"result.{memberdecl.DeclSymbol.Name}.Add(({memberdecl.GenericType.Name}){varArg});";
            StringBuilder builder = new StringBuilder();
            ITypeSymbol type = memberdecl.DeclType;
            if (memberdecl.DeclType.IsGenericType)
            {
                type = (type as INamedTypeSymbol).TypeArguments[0];
            }
            if (BsonGeneratorReadOperations.TryGetSimpleType(type, out string readOp))
            {
                builder.Append(GenerateSimpleReadOpAndAssign(memberdecl, readOp, typeArg, varArg, assignOp));
            }

            if (BsonGeneratorReadOperations.TryGetGeneratedType(type, out readOp))
            {
                builder.Append(GenerateGeneratedReadOpAndAssign(memberdecl, type, readOp, typeArg, varArg, assignGenericOp));
            }
            if (BsonGeneratorReadOperations.IsHardType(type))
            {
                builder.Append(GeneratehHardReadOpAndAssign(memberdecl, type, typeArg, varArg, assignOp));
            }
            return builder.ToString();
        }
        private string GenerateGeneratedReadOpAndAssign(MemberDeclarationMeta declinfo, ITypeSymbol sym, string readOp, string typeArg, string varArg, string assignOp, bool forceAssign = false)
        {
            var builder = new StringBuilder();
            if (forceAssign)
            {
                builder.Append($@"
                        {String.Format(readOp, $"{typeArg} {varArg}")}
                        {assignOp}
                        ");

                return builder.ToString();
            }


            if (declinfo.IsProperty)
            {
                builder.Append($@"
                        {String.Format(readOp, $"{typeArg} {varArg}")}
                        {assignOp}
                        ");

                return builder.ToString();
            }
            else
            {
                if (forceAssign)
                {
                    builder.Append($@"
                        {String.Format(readOp, $"{typeArg} {varArg}")}
                        {assignOp}
                        ");
                }
                else
                {
                    builder.Append($@"
                        {String.Format(readOp, $"result.{declinfo.DeclSymbol.Name}")}
                        ");
                }
            }
            return builder.ToString();
        }
        private string GenerateReadOtherDocument(ClassDeclMeta classdecl, MemberDeclarationMeta memberdecl)
        {

            StringBuilder builder = new StringBuilder();
            builder.Append($@"
                     if ( !GlobalSerializationHelperGenerated.{memberdecl.DeclType.Name}GeneratedSerializerStaticField.TryParse(ref reader, out var value)){{ return false;}}
                     result.{memberdecl.DeclSymbol.Name} = ({memberdecl.DeclType.Name})value;
            ");
            return builder.ToString();
        }
    }
}
