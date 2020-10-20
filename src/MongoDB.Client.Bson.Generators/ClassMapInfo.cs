using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDB.Client.Bson.Generators
{
    internal class MapFieldInfo 
    {
        private static int _id = 0;
        public int Id;
        public string BsonField { get; set; }
        public string BsonFieldAlias { get; set; }
        public string ClassField { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public string ShortType { get; set; }
        public string TypeAlias { get; set; }
        public string GenericType { get; set; }
        public string GenericShortType { get; set; }
        public string GenericTypeAlias { get; set; }
        public bool isDocument { get; set; }
        public MapFieldInfo()
        {
            BsonFieldAlias = null;
            GenericType = null;
            GenericShortType = null;
            GenericTypeAlias = null;
            Type = null;
            ShortType = null;
            isDocument = false;
            Id = _id++;
        }

    }

    internal class ClassMapInfo
    {
        public string ClassName { get; set; }
        public List<MapFieldInfo> MapedFields { get; set; }
        public ClassMapInfo()
        {
            MapedFields = new List<MapFieldInfo>();            
        }
    }
}
