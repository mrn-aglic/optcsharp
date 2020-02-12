using System.Linq;
using Newtonsoft.Json.Linq;
using TracingCore.Data;

namespace TracingCore.JsonMappers
{
    public static class PyTutorDataMapper
    {
        public static JObject ToJson(PyTutorData pyTutorData)
        {
            var jObject = new JObject();

            jObject.Add("code", JToken.FromObject(pyTutorData.Code));
            jObject.Add("trace", JToken.FromObject(pyTutorData.Trace.Select(PyTutorStepMapper.ToJson)));
            
            return jObject;
        }
    }
}