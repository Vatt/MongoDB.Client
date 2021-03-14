using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        public enum OpCtxType
        {
            Root,
            Condition,
            Case,
            Switch
        }
        public class OperationContext
        {
            public OpCtxType Type { get; }
            public int? Key { get; }
            public int? Offset { get; }
            public MemberContext Member { get; set; }
            public List<OperationContext> InnerOperations;
            public OperationContext(OpCtxType type, int key, MemberContext member)
            {
                Type = type;
                Key = key;
                Member = member;
                InnerOperations = new();
            }
            public OperationContext(OpCtxType type, int? key, int offset)
            {
                Key = key;
                Type = type;
                Offset = offset;
                InnerOperations = new();
            }
            public OperationContext(OpCtxType type, int offset)
            {
                Type = type;
                Offset = offset;
                InnerOperations = new();
            }
            public OperationContext(OpCtxType type, MemberContext member)
            {
                Type = type;
                Member = member;
            }
            public void AddCondition(MemberContext member)
            {
                InnerOperations.Add(new OperationContext(OpCtxType.Condition, member));
            }
            public void AddCase(int key, MemberContext member)
            {
                InnerOperations.Add(new OperationContext(OpCtxType.Case, key, member));
            }
            public void Add(OperationContext operation)
            {
                InnerOperations.Add(operation);
            }
        }
        public static bool ContextTreeGroupMembers(int offset, List<MemberContext> members, out List<MemberContext> conditions, out Dictionary<byte, List<MemberContext>> groups)
        {
            var canContinue = true;
            conditions = new List<MemberContext>();
            groups = new Dictionary<byte, List<MemberContext>>();
            foreach (var member in members)
            {
                var nameSpan = member.ByteName.Span;
                if ((nameSpan.Length - 1) < offset)
                {
                    canContinue = false;
                    conditions.Add(member);
                    continue;
                }
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
            return canContinue;
        }
        private static OperationContext CreateCaseContext(List<MemberContext> members, int inputKey, int inputOffset)
        {
            var caseOp = new OperationContext(OpCtxType.Case, inputKey, 0);
            if (members.Count == 1)
            {
                caseOp.Member = members[0];
                return caseOp;
            }
            var offset = inputOffset;
            var canContinue = ContextTreeGroupMembers(offset, members, out var conditions, out var groups);
            while (canContinue && groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                canContinue = ContextTreeGroupMembers(offset, members, out conditions, out groups);
            }
            foreach(var condition in conditions)
            {
                caseOp.AddCondition(condition);
            }
            if (groups.Count == 1 && groups.Values.ToArray()[0].Count == 1)
            {
                caseOp.AddCondition(groups.Values.ToArray()[0][0]);
            }
            else
            {
                var innerSwitch = CreateSwitchContext(groups, offset);
                caseOp.Add(innerSwitch);
            }

            return caseOp;
        }
        private static OperationContext CreateSwitchContext(Dictionary<byte, List<MemberContext>> groups, int inputOffset)
        {
            var switchOp = new OperationContext(OpCtxType.Switch, inputOffset);
            foreach (var pair in groups)
            {
                var groupKey = pair.Key;
                var groupValue = pair.Value;
                var caseOp = CreateCaseContext(groupValue, groupKey, inputOffset);
                switchOp.Add(caseOp);
            }
            return switchOp;
        }
        private static StatementSyntax[] ContextTreeTryParseOperations(ContextCore ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var offset = 0;
            var canContinue = ContextTreeGroupMembers(offset, ctx.Members, out var conditions, out var groups);
            while (canContinue && groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                canContinue = ContextTreeGroupMembers(offset, ctx.Members, out conditions, out groups);
            }
            var root = new OperationContext(OpCtxType.Root, offset);
            foreach(var condition in conditions)
            {
                root.AddCondition(condition);
            }
            foreach(var group in groups.Where(g => g.Value.Count == 1))
            {
                root.AddCase(group.Key, group.Value[0]);
            }
            foreach (var group in groups.Where(g => g.Value.Count > 1))
            {
                var caseOp = CreateCaseContext(group.Value, group.Key, offset);
                root.Add(caseOp);
            }
            
            return GenerateRoot(ctx, root, bsonType, bsonName);
        }
        private static SwitchStatementSyntax GenerateSwitch(ContextCore ctx, OperationContext host, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var sections = new List<SwitchSectionSyntax>();
            foreach (var operation in host.InnerOperations.Where(op => op.Type == OpCtxType.Case))
            {
                sections.Add(GenerateCase(ctx, operation, bsonType, bsonName));
            }
            return SF.SwitchStatement(ElementAccessExpr(bsonName, NumericLiteralExpr(host.Offset.Value)), new SyntaxList<SwitchSectionSyntax>(sections.ToArray()));
        }
        private static SwitchSectionSyntax GenerateCase(ContextCore ctx, OperationContext host, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            var label = new SyntaxList<SwitchLabelSyntax>(SF.CaseSwitchLabel(NumericLiteralExpr(host.Key.Value)));
            MemberContext member = host.Member;
            foreach(var operation in host.InnerOperations)
            {
                switch (operation.Type)
                {
                    case OpCtxType.Condition:
                        builder.AddRange(GenerateCondition(ctx, operation, bsonType, bsonName));
                        break;
                    case OpCtxType.Switch:
                        builder.Add(GenerateSwitch(ctx, operation, bsonType, bsonName));
                        break;
                }
            }
            if (member is not null)
            {
                if (TryGenerateParseEnum(member.StaticSpanNameToken, member.AssignedVariableToken, bsonName, member.NameSym, member.TypeSym, builder))
                {

                }
                else if (TryGenerateSimpleReadOperation(ctx, member, bsonType, bsonName, builder))
                {

                }
                else if (TryGenerateTryParseBson(member, bsonName, builder))
                {

                }
            }

            return SF.SwitchSection(label, new SyntaxList<StatementSyntax>(Block(builder.ToArray(), SF.BreakStatement())));
        }
        private static StatementSyntax[] GenerateCondition(ContextCore ctx, OperationContext host, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            MemberContext member = host.Member;
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
        private static StatementSyntax[] GenerateRoot(ContextCore ctx, OperationContext host, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            foreach(var condition in host.InnerOperations.Where(op => op.Type == OpCtxType.Condition))
            {
                var member = condition.Member;
                if (TryGenerateParseEnum(member.StaticSpanNameToken, member.AssignedVariableToken, bsonName, member.NameSym, member.TypeSym, builder))
                {

                }
                else if (TryGenerateSimpleReadOperation(ctx, member, bsonType, bsonName, builder))
                {

                }
                else if (TryGenerateTryParseBson(member, bsonName, builder))
                {

                }
            }
            //var label = new SyntaxList<SwitchLabelSyntax>(SF.CaseSwitchLabel(NumericLiteralExpr(host.Key.Value)));
            var sections = new List<SwitchSectionSyntax>();
            foreach (var operation in host.InnerOperations.Where(op => op.Type == OpCtxType.Case))
            {
                var label = new SyntaxList<SwitchLabelSyntax>(SF.CaseSwitchLabel(NumericLiteralExpr(operation.Key.Value)));
                sections.Add(GenerateCase(ctx, operation, bsonType, bsonName));
            }
            builder.Add(SF.SwitchStatement(ElementAccessExpr(bsonName, NumericLiteralExpr(host.Offset.Value)), new SyntaxList<SwitchSectionSyntax>(sections.ToArray())));
            return builder.ToArray();
        }
    }
}
