using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TracingCore.Data
{
    public class ClassHeapData : HeapData
    {
        public override HeapType HeapType => HeapType.Class;
        public string FullyQualifiedName { get; }
        public bool IsStatic { get; }
        public string[] Extends { get; }
        public Type Type { get; }

        public ClassHeapData(int heapId, bool isStatic, Type type, string fullyQualifiedName, string[] extends) :
            base(heapId, null)
        {
            FullyQualifiedName = fullyQualifiedName;
            IsStatic = isStatic;
            Extends = extends;
            Type = type;
        }

        public IEnumerable<MethodHeapData> FindMyMembersInHeap(ImmutableDictionary<int, HeapData> heap)
        {
            return heap.Values.OfType<MethodHeapData>().Where(x => x.EnclosingParentFullName == FullyQualifiedName);
        }
    }
}