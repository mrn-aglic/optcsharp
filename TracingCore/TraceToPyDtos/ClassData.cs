using System.Collections.Generic;
using System.Linq;
using TracingCore.Data;
using TracingCore.TreeRewriters;

namespace TracingCore.TraceToPyDtos
{
    public class ClassData : ITracePyDto
    {
        public HeapType OptHeapType = HeapType.Class;
        public string Name { get; }
        public string ExtendedClass { get; }
        public string FullyQualifiedPath { get; }
        public IList<MethodData> Methods { get; }
        public LineData LineData { get; }
        public bool HasMain { get; }

        public bool ClassLoaded { get; set; }

        public ClassData(string name, string extendedClass, string fullyQualifiedPath, IList<MethodData> methods, LineData lineData)
        {
            Name = name;
            ExtendedClass = extendedClass;
            FullyQualifiedPath = fullyQualifiedPath;
            Methods = methods;
            LineData = lineData;
            HasMain = Methods.Any(x => x.IsMain);
            ClassLoaded = false;
        }

        public HeapData ToHeapData()
        {
            throw new System.NotImplementedException();
        }
    }
}