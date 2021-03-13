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
        public readonly struct GroupContext
        {
            public readonly bool NeedCondition;
            public readonly MemberContext Member;
            public GroupContext(MemberContext member)
            {
                NeedCondition = false;
                Member = member;
            }
            public GroupContext(bool needContext, MemberContext member)
            {
                NeedCondition = needContext;
                Member = member;
            }
        }
        public static Dictionary<byte, List<GroupContext>> ContextTreeGroupMembers(int offset, List<MemberContext> members, out bool canContinue)
        {
            canContinue = true;
            var groups = new Dictionary<byte, List<GroupContext>>();
            //foreach(var member in members)
            //{
            //    if ((member.ByteName.Length - 1) < offset)
            //    {
            //        canContinue = false;
            //        break;
            //    }
            //}
            foreach (var member in members)
            {
                var nameSpan = member.ByteName.Span;
                if ((nameSpan.Length - 1) < offset)
                {
                    canContinue = false;
                    if (groups.ContainsKey((byte)(nameSpan.Length - 1)))
                    {
                        groups[(byte)(nameSpan.Length - 1)].Add(new GroupContext(true, member));
                    }
                    else
                    {
                        groups.Add((byte)(nameSpan.Length - 1), new() { new GroupContext(true, member) });
                    }
                    continue;
                }
                //if ((nameSpan.Length - 1) >= offset)
                //{
                //    if (groups.ContainsKey(key))
                //    {
                //        groups[key].Add(new GroupContext(true, member));
                //    }
                //    else
                //    {
                //        groups.Add(key, new() { new GroupContext(true, member) });
                //    }
                //    continue;
                //}
                
                var key = nameSpan[offset];
                if (groups.ContainsKey(key))
                {
                    groups[key].Add(new GroupContext(member));
                }
                else
                {
                    groups.Add(key, new() { new GroupContext(member) });
                }
            }
            return groups;
        }
        public static SwitchCaseContextBase CreateContextTreeSwitchContext(List<MemberContext> members, int inputOffset, MultiContext host)
        {
            var offset = inputOffset;
            var groups = ContextTreeGroupMembers(offset, members, out var canContinue);
            while (canContinue && groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                groups = ContextTreeGroupMembers(offset, members, out canContinue);
            }

            foreach (var pair in groups)
            {
                var key = pair.Key;
                var value = pair.Value;
                if (value.Count == 1)
                {
                    var node = value[0];
                    if (node.NeedCondition)
                    {
                        host.Add(new ConditionContext(node.Member));
                    }
                    else
                    {
                        //host.Add(new CaseContext(key, node.Member));
                        host.Add(new ConditionContext(node.Member));
                    }

                }
                else
                {
                    var values = value.Select(g => g.Member).ToList();
                    var groups1 = ContextTreeGroupMembers(offset, values, out canContinue);
                    var offset1 = 0;// offset;
                    while (canContinue && groups1.Values.Count == 1 && groups1.Values.First().Count > 1)
                    {
                        offset1 += 1;
                        var values1 = value.Select(g => g.Member).ToList();
                        groups1 = ContextTreeGroupMembers(offset1, values1, out canContinue);
                    }
                    if (groups1.Values.Count == 1 && groups1.Values.First().Count > 1)
                    {
                        var temp1 = new MultiContext(key, offset1);
                        var conditions = value.Where(g => g.NeedCondition);
                        foreach (var cond in conditions)
                        {
                            host.Add(new ConditionContext(cond.Member));
                        }
                        var other = value.Where(g => g.NeedCondition == false).Select(g => g.Member).ToList();
                        //temp1.Add(CreateContextTreeSwitchContext(other, offset1, temp1));
                        CreateContextTreeSwitchContext(other, offset1, temp1);
                        host.Add(temp1);
                        continue;
                    }
                    var temp = new SwitchContext(key, offset1);
                    host.Add(CreateContextTreeSwitchContext(values, offset1, temp));
                }
            }
            return host;
        }
        public static SwitchCaseContextBase CreateContextTreeSwitchContext(List<MemberContext> members, int inputOffset, SwitchContext host)
        {
            var offset = inputOffset;
            var groups = ContextTreeGroupMembers(offset, members, out var canContinue);
            while (canContinue && groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                groups = ContextTreeGroupMembers(offset, members, out canContinue);
            }

            foreach (var pair in groups)
            {
                var key = pair.Key;
                var value = pair.Value;
                if (value.Count == 1)
                {
                    var node = value[0];
                    if (node.NeedCondition)
                    {
                        host.Add(new ConditionContext(node.Member));
                    }
                    else
                    {
                        host.Add(new CaseContext(key, node.Member));
                    }
                    //host.Add(new CaseContext(key, node.Member));
                }
                else
                {
                    var values = value.Select(g => g.Member).ToList();
                    var groups1 = ContextTreeGroupMembers(offset, values, out canContinue);
                    var offset1 = 0;// offset;
                    while (canContinue && groups1.Values.Count == 1 && groups1.Values.First().Count > 1)
                    {
                        offset1 += 1;
                        var values1 = value.Select(g => g.Member).ToList();
                        groups1 = ContextTreeGroupMembers(offset1, values1, out canContinue);
                    }
                    if (groups1.Values.Count == 1 && groups1.Values.First().Count > 1)
                    {
                        var temp1 = new MultiContext(key, offset1);
                        var values1 = value.Select(g => g.Member).ToList();
                        host.Add(CreateContextTreeSwitchContext(values1, offset1, temp1));
                        continue;
                    }
                    var temp = new SwitchContext(key, offset1);
                    host.Add(CreateContextTreeSwitchContext(values, offset1, temp));
                }
            }
            return host;
        }

        private static StatementSyntax[] ContextTreeTryParseOperations(ContextCore ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var offset = 0;
            var groups = ContextTreeGroupMembers(offset, ctx.Members, out var canContinue);
            while (canContinue && groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                groups = ContextTreeGroupMembers(offset, ctx.Members, out canContinue);
            }
            var root = new SwitchContext(offset);
            var genCtx = CreateContextTreeSwitchContext(ctx.Members, offset, root);
            return GenerateContextTreeSwitch(ctx, root, bsonType, bsonName);
        }
        private static StatementSyntax[] GenerateContextTreeSwitch(ContextCore ctx, HostContext switchCtx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var sections = new List<SwitchSectionSyntax>();
            foreach (var node in switchCtx.Endnodes)
            {
                
                var builder = ImmutableList.CreateBuilder<StatementSyntax>();
                if (node is SwitchContext nodectx)
                {
                    var label = new SyntaxList<SwitchLabelSyntax>(SF.CaseSwitchLabel(NumericLiteralExpr(node.Key.Value)));
                    sections.Add(SF.SwitchSection(label, new SyntaxList<StatementSyntax>(
                        Block(GenerateContextTreeSwitch(ctx, nodectx, bsonType, bsonName), SF.BreakStatement()))));
                }else if (node is MultiContext multiCtx)
                {
                    var label = new SyntaxList<SwitchLabelSyntax>(SF.CaseSwitchLabel(NumericLiteralExpr(node.Key.Value)));
                    List<StatementSyntax> statements = new();
                    foreach (var endnode in multiCtx.Endnodes)
                    {
                        switch (endnode)
                        {
                            case MultiContext multiContext:
                                statements.AddRange(GenerateContextTreeSwitch(ctx, multiContext, bsonType, bsonName));
                                break;
                            case SwitchContext switchContext:
                                statements.AddRange(GenerateContextTreeSwitch(ctx, switchContext, bsonType, bsonName));
                                break;
                            case ConditionContext condCtx:
                                var member = condCtx.Member;
                                if (TryGenerateParseEnum(member.StaticSpanNameToken, member.AssignedVariableToken, bsonName, member.NameSym, member.TypeSym, builder))
                                {
                                    statements.AddRange(builder.ToArray());
                                }
                                else if (TryGenerateSimpleReadOperation(ctx, member, bsonType, bsonName, builder))
                                {
                                    statements.AddRange(builder.ToArray());
                                }
                                else if (TryGenerateTryParseBson(member, bsonName, builder))
                                {
                                    statements.AddRange(builder.ToArray());
                                }
                                break;
                        }
                    }
                    sections.Add(SF.SwitchSection(label, new SyntaxList<StatementSyntax>(
                        Block(statements.ToArray(), SF.BreakStatement()))));
                }
                else if (node is ConditionContext condCtx)
                {
                    MemberContext member = condCtx.Member;
                    if (TryGenerateParseEnum(member.StaticSpanNameToken, member.AssignedVariableToken, bsonName, member.NameSym, member.TypeSym, builder))
                    {

                    }
                    else if (TryGenerateSimpleReadOperation(ctx, member, bsonType, bsonName, builder))
                    {

                    }
                    else if (TryGenerateTryParseBson(member, bsonName, builder))
                    {

                    }
                    return builder.ToArray();
                }
                else if (node is CaseContext caseCtx)
                {
                    var label = new SyntaxList<SwitchLabelSyntax>(SF.CaseSwitchLabel(NumericLiteralExpr(node.Key.Value)));
                    MemberContext member = caseCtx.Member;
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