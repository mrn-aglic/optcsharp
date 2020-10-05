using System;
using System.Collections.Generic;
using TracingCore.Common;
using TracingCore.Data;

namespace TracingCore.TraceToPyDtos
{
    public class StructData : TypeDeclarationData
    {
        public override bool IsStruct => true;
        public override bool IsClass => false;

        public StructData(string name, bool isStatic, string[] extendedTypes, string fullyQualifiedPath,
            IList<MethodData> methods, LineData lineData,
            Type type) : base(name, isStatic, extendedTypes, fullyQualifiedPath, methods, lineData, type)
        {
        }
        
        public override HeapData ToHeapData()
        {
            throw new NotImplementedException();
        }
    }
}