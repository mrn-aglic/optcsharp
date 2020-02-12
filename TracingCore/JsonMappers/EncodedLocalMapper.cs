using System;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json.Linq;
using TracingCore.Data;

namespace TracingCore.JsonMappers
{
    public static class EncodedLocalMapper
    {
        public static JToken ToJson(OptVariableData optVariableData)
        {
            var value = optVariableData.Value;
            return optVariableData.IsRef && !(optVariableData.Value is string)
                ? JArray.FromObject(new object[]
                {
                    "REF",
                    optVariableData.HeapId
                })
                : value != null
                    ? JToken.FromObject(value)
                    : null;
        }
    }
}