using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TracingCore;
using TracingCore.JsonMappers;

namespace OptLocal.Areas.Default.Controllers
{
    [Area("Default")]
    public class TraceApiController : Controller
    {
        private const string CompilationName = "opt-compilation";

        [HttpGet, Route("/api/getcsharptrace")]
        public JObject GetCSharpTrace(
            [FromQuery] string user_script,
            [FromQuery] string options_json,
            [FromQuery] string raw_input_json)
        {
            var inputs = raw_input_json == null
                ? new List<string>()
                : JArray.Parse(raw_input_json).ToObject<List<string>>();
            var optBackend = new OptBackend(user_script, inputs);

            var compilationResult = optBackend.Compile(CompilationName, true);
            var pyTutorData = optBackend.Trace(compilationResult.Root, compilationResult);

            return PyTutorDataMapper.ToJson(pyTutorData);
        }
    }
}