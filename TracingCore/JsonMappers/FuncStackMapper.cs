using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using TracingCore.Data;

namespace TracingCore.JsonMappers
{
    public static class FuncStackMapper
    {
        private static JObject MapEncodedLocals(ImmutableList<OptVariableData> encodedLocals)
        {
            var jObject = new JObject();

            encodedLocals.ForEach(local =>
            {
                jObject.Add(local.Name, EncodedLocalMapper.ToJson(local));
            });

            return jObject;
        }

        public static JObject ToJson(FuncStack funcStack)
        {
            var jObject = new JObject();

            jObject.Add("func_name", funcStack.FuncName);
            jObject.Add("encoded_locals", MapEncodedLocals(funcStack.EncodedLocals));
            jObject.Add("ordered_varnames", JToken.FromObject(funcStack.OrderedVarNames));
            jObject.Add("parent_frame_id_list", JToken.FromObject(funcStack.ParentFrameIdList));
            jObject.Add("is_highlighted", JToken.FromObject(funcStack.IsHighlighted));
            jObject.Add("is_zombie", JToken.FromObject(funcStack.IsZombie));
            jObject.Add("is_parent", JToken.FromObject(funcStack.IsParent));
            jObject.Add("unique_hash", JToken.FromObject(funcStack.UniqueHash));
            jObject.Add("frame_id", JToken.FromObject(funcStack.FrameId));

            return jObject;
        }
    }
}