using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static Dictionary<byte, List<MemberContext>> ContextTreeGroupMembers(int offset, List<MemberContext> members)
        {
            var groups = new Dictionary<byte, List<MemberContext>>();
            foreach (var member in members)
            {
                var nameSpan = member.ByteName.Span;
                var key = nameSpan[offset];
                if (groups.ContainsKey(key))
                {
                    groups[key].Add(member);
                }
                else
                {
                    groups.Add(key, new() { member });
                }
            }
            return groups;
        }

        public static SwitchCaseContextBase CreateContextTreeSwitchContext(List<MemberContext> members, int inputOffset, SwitchContext host)
        {
            var offset = inputOffset;
            var groups = ContextTreeGroupMembers(offset, members);
            while (groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                groups = ContextTreeGroupMembers(offset, members);
            }

            foreach (var pair in groups)
            {
                var key = pair.Key;
                var value = pair.Value;
                if (value.Count == 1)
                {
                    host.Add(new CaseContext(key, value[0]));
                }
                else
                {
                    var groups1 = ContextTreeGroupMembers(offset, value);
                    var offset1 = 0;
                    while (groups1.Values.Count == 1 && groups1.Values.First().Count > 1)
                    {
                        offset1 += 1;
                        groups1 = ContextTreeGroupMembers(offset1, value);
                    }
                    var temp = new SwitchContext(key, offset1);
                    host.Add(CreateContextTreeSwitchContext(value, offset1, temp));
                }
            }
            return host;

        }

        private static StatementSyntax[] ContextTreeTryParseOperations(ContextCore ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var offset = 0;
            var groups = ContextTreeGroupMembers(offset, ctx.Members);
            while (groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                groups = ContextTreeGroupMembers(offset, ctx.Members);
            }
            var root = new SwitchContext(offset);
            var genCtx = CreateContextTreeSwitchContext(ctx.Members, offset, root);
            return GenerateContextTreeSwitch(ctx, root, bsonType, bsonName);
        }
        private static StatementSyntax[] GenerateContextTreeSwitch(ContextCore ctx, SwitchContext switchCtx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var sections = new List<SwitchSectionSyntax>();
            foreach (var node in switchCtx.Endnodes)
            {
                var label = new SyntaxList<SwitchLabelSyntax>(SF.CaseSwitchLabel(NumericLiteralExpr(node.Key.Value)));
                var builder = ImmutableList.CreateBuilder<StatementSyntax>();
                if (node is SwitchContext nodectx)
                {
                    sections.Add(SF.SwitchSection(label, new SyntaxList<StatementSyntax>(
                        Block(GenerateContextTreeSwitch(ctx, nodectx, bsonType, bsonName), SF.BreakStatement()))));
                }
                else
                {
                    var member = ((CaseContext)node).Member;
                    if (TryGenerateParseEnum(member.StaticSpanNameToken, member.AssignedVariableToken, bsonName, member.NameSym, member.TypeSym, builder))
                    {

                    }
                    else if (TryGenerateSimpleReadOperation(ctx, member, bsonType, bsonName, builder))
                    {

                    }
                    else if (TryGenerateTryParseBson(member, bsonName, builder))
                    {

                    }
                    sections.Add(SF.SwitchSection(label, new SyntaxList<StatementSyntax>(Block(builder.ToArray(), SF.BreakStatement()))));
                }

            }
            return new StatementSyntax[]
            {
                SF.SwitchStatement(ElementAccessExpr(bsonName, NumericLiteralExpr(switchCtx.Offset)), new SyntaxList<SwitchSectionSyntax>(sections.ToArray()))
            };
        }
    }
}