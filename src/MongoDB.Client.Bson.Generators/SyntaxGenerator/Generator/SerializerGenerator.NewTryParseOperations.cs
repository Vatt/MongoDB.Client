﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Buffers.Binary;
using System.Collections.Generic;

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
        public class CaseContext : SwitchCaseContextBase
        {
            public int Offset { get; }
            public MemberContext Member { get; }
            public CaseContext(int key, int offset, MemberContext ctx) : base(key/*, offset*/)
            {
                Offset = offset;
                Member = ctx;
            }
        }
        public class SwitchContext : SwitchCaseContextBase
        {
            public int Offset { get; }
            public List<SwitchCaseContextBase> Endnodes { get; }
            public bool IsSingleEndNode => Endnodes.Count == 1 && Endnodes[0] is CaseContext;
            public SwitchContext(int key, int offset) : base(key)
            {
                Offset = offset;
                Endnodes = new();
            }
            public SwitchContext(int offset) : base(null)
            {
                Offset = offset;
                Endnodes = new();
            }
            public void Add(SwitchCaseContextBase node)
            {
                Endnodes.Add(node);
            }
        }
        public static Dictionary<int, List<MemberContext>> GroupMembers(int offset, List<MemberContext> members)
        {
            var groups = new Dictionary<int, List<MemberContext>>();
            foreach (var member in members)
            {
                var nameSpan = member.ByteName.Span;
                if ((nameSpan.Length - offset) >= 4)
                {
                    var intPart = BinaryPrimitives.ReadInt32LittleEndian(nameSpan.Slice(offset, 4));
                    if (groups.ContainsKey(intPart))
                    {
                        groups[intPart].Add(member);
                    }
                    else
                    {
                        groups.Add(intPart, new() { member });
                    }
                }
                else
                {
                    if ((nameSpan.Length - offset) >= 2)
                    {
                        var shortPart = BinaryPrimitives.ReadInt16LittleEndian(nameSpan.Slice(offset, 2));
                        if (groups.ContainsKey(shortPart))
                        {
                            groups[shortPart].Add(member);
                        }
                        else
                        {
                            groups.Add(shortPart, new() { member });
                        }
                    }
                    else
                    {
                        var bytePart = nameSpan[0];
                        if (groups.ContainsKey(bytePart))
                        {
                            groups[bytePart].Add(member);
                        }
                        else
                        {
                            groups.Add(bytePart, new() { member });
                        }
                    }
                }
            }
            return groups;
        }
        public static SwitchCaseContextBase CreateSwitchContext(List<MemberContext> members, int inputOffset, SwitchContext host)
        {
            var offset = inputOffset;
            var groups = GroupMembers(offset, members);
            if (groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                groups = GroupMembers(offset, members);
            }

            foreach(var pair in groups)
            {
                var key = pair.Key;
                var value = pair.Value;
                if (value.Count  == 1)
                {
                    host.Add(new CaseContext(key, offset, value[0]));
                }
                else
                {
                    var temp = new SwitchContext(key, offset);
                    host.Add(CreateSwitchContext(value, offset, temp));
                }
            }
            return host;

        }
        private static StatementSyntax[] NewTryParseOperations(ContextCore ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var offset = 0;
            var groups = GroupMembers(offset, ctx.Members);
            while(groups.Values.Count == 1 && groups.Values.First().Count > 1)
            {
                offset += 1;
                groups = GroupMembers(offset, ctx.Members);
            }
            var root = new SwitchContext(offset);
            var genCtx = CreateSwitchContext(ctx.Members, 0, root);
            return Operations(ctx, bsonType, bsonName);
        }
        private static StatementSyntax[] NameLengthFirst(ContextCore ctx)
        {
            var namesMap = new Dictionary<int, List<MemberContext>>();
            foreach(var member in ctx.Members)
            {
                if(namesMap.TryGetValue(member.ByteName.Length, out var list))
                {
                    list.Add(member);
                }
                else
                {
                    namesMap.Add(member.ByteName.Length, new(){ member });
                }
            }
            return default;
        }
    }
}