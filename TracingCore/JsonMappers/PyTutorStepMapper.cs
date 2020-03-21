using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using TracingCore.Common;
using TracingCore.Data;

namespace TracingCore.JsonMappers
{
    public static class PyTutorStepMapper
    {
        private static bool _useFullName = true;
        private static bool _ignoreNull = true;

        private static BindingFlags _fieldBindingFlags =
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.DeclaredOnly;

        private static BindingFlags _propertyBindingFlags =
            BindingFlags.DeclaredOnly |
            BindingFlags.NonPublic | // Should we include non-public properties?
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.Instance;

        private static string _backupPropPrefix;

        public static void RegisterConfig(InstrumentationConfig instrumentationConfig)
        {
            _backupPropPrefix = instrumentationConfig.Property.BackupNamePrefix;
        }

        private static JToken ToJElement(string prop, int val)
        {
            return new JArray {prop, val};
        }

        private static int? FindInHeap(ImmutableDictionary<int, HeapData> heap, object obj)
        {
            return heap.Values.FirstOrDefault(x => x.Value == obj)?.HeapId;
        }

        private static JToken PopulateJArray(JArray jArray, ImmutableDictionary<int, HeapData> heap,
            IEnumerable<(string, object)> value)
        {
            var instanceString = HeapType.Instance.ToString().ToUpper();
            var classString = HeapType.Class.ToString().ToUpper();
            var listString = HeapType.List.ToString().ToUpper();
            foreach (var (name, obj) in value)
            {
                if (!_ignoreNull || obj != null)
                {
                    var heapId = FindInHeap(heap, obj);
                    var valueToken = heapId.HasValue
                        ? ToJElement("REF", heapId.Value)
                        : obj == null
                            ? null
                            : JToken.FromObject(obj);

                    switch (jArray.First().ToString())
                    {
                        case var type when type == instanceString ||
                                           type == classString:
                            jArray.Add(new JArray {name, valueToken});
                            break;
                        case var type when type == listString:
                            jArray.Add(valueToken);
                            break;
                    }
                }
            }

            return jArray;
        }

        public static JToken CreateJArray(ImmutableDictionary<int, HeapData> heap, IEnumerable<object> value)
        {
            var heapTypeString = HeapType.List.ToString().ToUpper();

            var jArray = new JArray {heapTypeString};

            var values = value.ToList();
            return PopulateJArray(jArray, heap, Enumerable.Repeat("REF", values.Count).Zip(values));
        }

        public static JToken CreateJInstance(ImmutableDictionary<int, HeapData> heap, HeapData value)
        {
            var varVal = value.Value;
            var fullName = varVal.GetType().ToString();
            var instanceName = _useFullName ? fullName : RoslynHelper.ClassNameFromFullName(fullName);

            var heapTypeString = value.HeapType.ToString().ToUpper();

            var jArray = new JArray {heapTypeString, instanceName};
            var type = varVal.GetType();
            var prefixLength = _backupPropPrefix.Length;
            var properties = type.GetProperties(_propertyBindingFlags)
                .Where(x => x.Name.StartsWith(_backupPropPrefix))
                .Select(x => (x.Name.Substring(prefixLength), x.GetValue(varVal)))
                .ToList();
            var fields = type.GetFields(_fieldBindingFlags).Select(x => (x.Name, x.GetValue(varVal))).ToList();

            return PopulateJArray(jArray, heap, fields.Concat(properties));
        }

        public static JToken CreateJFunction(MethodHeapData value)
        {
            var declaration = value.Declaration;
            var heapTypeString = value.HeapType.ToString().ToUpper();

            return new JArray {heapTypeString, declaration, null};
        }

        public static JToken CreateJClass(ImmutableDictionary<int, HeapData> heap, ClassHeapData value)
        {
            var heapTypeString = value.HeapType.ToString().ToUpper();
            var extends = JArray.FromObject(value.Extends);

            var jArray = new JArray
                {heapTypeString, $"{value.FullyQualifiedName + (value.IsStatic ? " static" : "")}", extends};

            var methods = heap.Values.OfType<MethodHeapData>()
                .Where(x => x.EnclosingParentFullName == value.FullyQualifiedName);

            foreach (var method in methods)
            {
                jArray.Add(new JArray {method.FuncName, new JArray {"REF", method.HeapId}});
            }

            if (!value.IsStatic) return jArray;

            var type = value.Type;
            var fields = type.GetFields(_fieldBindingFlags).Select(x => (x.Name, x.GetValue(null)))
                .ToList();
            return PopulateJArray(jArray, heap, fields);
        }

        public static JObject HeapToJson(ImmutableDictionary<int, HeapData> heap)
        {
            JObject json = new JObject();
            foreach (var element in heap)
            {
                var heapType = element.Value.HeapType;

                switch (heapType)
                {
                    case HeapType.List:
                        var array = element.Value.Value;
                        json.Add(element.Key.ToString(), CreateJArray(heap, (IEnumerable<object>) array));
                        break;
                    case HeapType.Instance:
                        json.Add(element.Key.ToString(), CreateJInstance(heap, element.Value));
                        break;
                    case HeapType.Function:
                        json.Add(element.Key.ToString(), CreateJFunction(element.Value as MethodHeapData));
                        break;
                    case HeapType.Class:
                        json.Add(element.Key.ToString(), CreateJClass(heap, element.Value as ClassHeapData));
                        break;
                }
            }

            return json;
        }

        public static JObject ToJson(IPyTutorStep pyTutorStep)
        {
            var jObject = new JObject();
            jObject.Add("event", JToken.FromObject(pyTutorStep.Event));

            switch (pyTutorStep)
            {
                case PyTutorStep pyStep:
                    jObject.Add("stdout", JToken.FromObject(pyStep.StdOut));
                    jObject.Add("line", JToken.FromObject(pyStep.Line));
                    if (pyStep.ExceptionMsg != null)
                    {
                        jObject.Add("exception_msg", JToken.FromObject(pyStep.ExceptionMsg));
                    }
                    jObject.Add("stack_to_render",
                        JToken.FromObject(pyStep.StackToRender.Select(FuncStackMapper.ToJson).Reverse()));
                    jObject.Add("globals", JToken.FromObject(pyStep.Globals));
                    jObject.Add("ordered_globals", JToken.FromObject(pyStep.OrderedGlobals));
                    jObject.Add("func_name", JToken.FromObject(pyStep.FuncName));
                    jObject.Add("heap", pyStep.JHeap);
                    break;
                case PyTutorRawInputStep pyTutorRawInputStep:
                    jObject.Add("prompt", JToken.FromObject(pyTutorRawInputStep.Prompt));
                    break;
            }

            return jObject;
        }
    }
}