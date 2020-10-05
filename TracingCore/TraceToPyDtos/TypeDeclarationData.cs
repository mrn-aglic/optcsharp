using System;
using System.Collections.Generic;
using TracingCore.Common;
using TracingCore.Data;

namespace TracingCore.TraceToPyDtos
{
    public abstract class TypeDeclarationData : ITracePyDto
    {
        public HeapType OptHeapType = HeapType.Class;
        public string Name { get; }
        public bool IsStatic { get; }
        public string[] ExtendedTypes { get; }
        public string FullyQualifiedPath { get; }
        public IList<MethodData> Methods { get; }
        public LineData LineData { get; }
        public Type Type { get; }
        public abstract bool IsStruct { get; }
        public abstract bool IsClass { get; }

        protected TypeDeclarationData(string name, bool isStatic, string[] extendedTypes, string fullyQualifiedPath,
            IList<MethodData> methods, LineData lineData,
            Type type)
        {
            Name = name;
            IsStatic = isStatic;
            ExtendedTypes = extendedTypes;
            FullyQualifiedPath = fullyQualifiedPath;
            Methods = methods;
            LineData = lineData;
            Type = type;
        }

        public HeapData AppendType(Type type)
        {
            return null;
        }

        public abstract HeapData ToHeapData();
    }
}