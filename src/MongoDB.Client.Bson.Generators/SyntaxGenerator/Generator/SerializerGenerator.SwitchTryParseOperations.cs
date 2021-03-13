using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Buffers.Binary;
using System.Collections.Generic;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{

    internal static partial class SerializerGenerator
    {
        public abstract class SwitchCaseContextBase
        {
            public int? Key { get; }
            public SwitchCaseContextBase(int? key)
            {
                Key = key;
            }
        }
        public class ConditionContext : SwitchCaseContextBase
        {
            public MemberContext Member { get; }
            public ConditionContext(MemberContext member) : base(null)
            {
                Member = member;
            }
        }
        public class CaseContext : SwitchCaseContextBase
        {
            public MemberContext Member { get; }
            public CaseContext(int key, MemberContext ctx) : base(key)
            {
                Member = ctx;
            }
        }
        public class HostContext : SwitchCaseContextBase
        {
            public int Offset { get; }
            public List<SwitchCaseContextBase> Endnodes { get; }
            public bool IsSingleEndNode => Endnodes.Count == 1 && Endnodes[0] is CaseContext;
            public HostContext(int key, int offset) : base(key)
            {
                Offset = offset;
                Endnodes = new();
            }
            public HostContext(int offset) : base(null)
            {
                Offset = offset;
                Endnodes = new();
            }
            public void Add(SwitchCaseContextBase node)
            {
                Endnodes.Add(node);
            }
            public void AddRange(IEnumerable<SwitchCaseContextBase> nodes)
            {
                Endnodes.AddRange(nodes);
            }
        }
        public class SwitchContext : HostContext
        {
            public SwitchContext(int key, int offset) : base(key, offset)
            {

            }
            public SwitchContext(int offset) : base(offset)
            {
  
            }
            //public int Offset { get; }
            //public List<SwitchCaseContextBase> Endnodes { get; }
            //public bool IsSingleEndNode => Endnodes.Count == 1 && Endnodes[0] is CaseContext;
            //public SwitchContext(int key, int offset) : base(key)
            //{
            //    Offset = offset;
            //    Endnodes = new();
            //}
            //public SwitchContext(int offset) : base(null)
            //{
            //    Offset = offset;
            //    Endnodes = new();
            //}
            //public void Add(SwitchCaseContextBase node)
            //{
            //    Endnodes.Add(node);
            //}
        }
        public class MultiContext : HostContext
        {
            public MultiContext(int key, int offset) : base(key, offset)
            {
          
            }
            //public List<SwitchCaseContextBase> Endnodes { get; }
            //public int Offset { get; }
            //public MultiContext(int key, int offset) : base(key)
            //{
            //    Offset = offset;
            //    Endnodes = new();
            //}
            //public void Add(SwitchCaseContextBase node)
            //{
            //    Endnodes.Add(node);
            //}
        }
        public static Dictionary<int, List<MemberContext>> GroupMembers(int offset, List<MemberContext> members)
        {
            var groups = new Dictionary<int, List<MemberContext>>();
            foreach (var member in members)
            {
                var nameSpan = member.ByteName.Span;
                nameSpan = nameSpan.Slice(offset);
                int key = default;
                if (nameSpan.Length >= sizeof(int))
                {
                    var cast = MemoryMarshal.Cast<byte, int>(nameSpan);
                    key =  cast[0];
                }
                else
                {
                    Span<byte> buffer = stackalloc byte[sizeof(int)];
                    nameSpan.CopyTo(buffer);
                    var cast = MemoryMarshal.Cast<byte, int>(buffer);
                    key = cast[0];
                }
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
        public static SwitchCaseContextBase CreateSwitchContext(List<MemberContext> members, int inputOffset, SwitchContext host)
        {
            var offset = inputOffset;
            var groups = GroupMembers(offset, members);
            while (groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                groups = GroupMembers(offset, members);
            }

            foreach (var pair in groups)
            {
                var key = pair.Key;
                var value = pair.Value;
                if (value.Count  == 1)
                {
                    host.Add(new CaseContext(key, value[0]));
                }
                else
                {
                    var groups1 = GroupMembers(offset, value);
                    var offset1 = 0;
                    while (groups1.Values.Count == 1 && groups1.Values.First().Count > 1)
                    {
                        offset1 += 1;
                        groups1 = GroupMembers(offset1, value);
                    }
                    var temp = new SwitchContext(key, offset1);
                    host.Add(CreateSwitchContext(value, offset1, temp));
                }
            }
            return host;

        }
        private static StatementSyntax[] SwitchTryParseOperations(ContextCore ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var offset = 0;
            var groups = GroupMembers(offset, ctx.Members);
            while(groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                groups = GroupMembers(offset, ctx.Members);
            }
            var root = new SwitchContext(offset);
            var genCtx = CreateSwitchContext(ctx.Members, offset, root);
            return GenerateSwitch(ctx, root, 1, bsonType, bsonName);
        }
        private static StatementSyntax[]  GenerateSwitch(ContextCore ctx, SwitchContext switchCtx, int testVarCnt, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var head = SF.SwitchStatement(IdentifierName("test"));
            var sections = new List<SwitchSectionSyntax>();
            foreach(var node in switchCtx.Endnodes)
            {
                var label = new SyntaxList<SwitchLabelSyntax>(SF.CaseSwitchLabel(NumericLiteralExpr(node.Key.Value)));
                var builder = ImmutableList.CreateBuilder<StatementSyntax>();
                if (node is SwitchContext nodectx)
                {
                    //testVarCnt += 1;
                    //var testAssigment = VarLocalDeclarationStatement(Identifier($"testVar{testVarCnt}"), BinaryPrimitivesReadInt32LittleEndian(bsonName));
                    sections.Add(SF.SwitchSection(label, new SyntaxList<StatementSyntax>(
                        Block(GenerateSwitch(ctx, nodectx, testVarCnt + 1, bsonType, bsonName), SF.BreakStatement()))));
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
                ///IfNotReturnFalse(TryGetIntFromNameSpan(bsonName, switchCtx.Offset, VarVariableDeclarationExpr(Identifier($"testVar{testVarCnt}")))),
                VarLocalDeclarationStatement(Identifier($"testVar{testVarCnt}"), GetIntFromNameSpan(bsonName, switchCtx.Offset)),
                SF.SwitchStatement(IdentifierName($"testVar{testVarCnt}"), new SyntaxList<SwitchSectionSyntax>(sections.ToArray()))
            };
        }
    }
}