using System;
using System.Collections.Generic;
using System.Linq;
using TracingCore.Common;
using TracingCore.Data;

namespace TracingCore.TraceToPyDtos
{
    public class ClassData : TypeDeclarationData
    {
        public bool HasMain { get; }
        public override bool IsStruct => false;
        public override bool IsClass => true;

        public ClassData(string name, bool isStatic, string[] extendedTypes, string fullyQualifiedPath,
            IList<MethodData> methods, LineData lineData,
            Type type) : base(name, isStatic, extendedTypes, fullyQualifiedPath, methods, lineData, type)
        {
            HasMain = Methods.Any(x => x.IsMain);
        }

        public override HeapData ToHeapData()
        {
            throw new NotImplementedException();
        }
    }
}