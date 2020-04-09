using System;
using System.Collections.Generic;
using System.Linq;
using TracingCore.Common;
using TracingCore.Data;

namespace TracingCore.TraceToPyDtos
{
    public class ClassData : ITracePyDto
    {
        public HeapType OptHeapType = HeapType.Class;
        public string Name { get; }
        public bool IsStatic { get; }
        public string[] ExtendedTypes { get; }
        public string FullyQualifiedPath { get; }
        public IList<MethodData> Methods { get; }
        public LineData LineData { get; }
        public bool HasMain { get; }
        public Type Type { get; }

        public ClassData(string name, bool isStatic, string[] extendedTypes, string fullyQualifiedPath,
            IList<MethodData> methods, LineData lineData,
            Type type)
        {
            Name = name;
            IsStatic = isStatic;
            ExtendedTypes = extendedTypes;
            FullyQualifiedPath = fullyQualifiedPath;
            Methods = methods;
            LineData = lineData;
            HasMain = Methods.Any(x => x.IsMain);
            Type = type;
        }

        public HeapData AppendType(Type type)
        {
            return null;
        }

        public HeapData ToHeapData()
        {
            throw new System.NotImplementedException();
        }
    }
}