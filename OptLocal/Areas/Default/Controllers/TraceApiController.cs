using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TracingCore;
using TracingCore.JsonMappers;
using TracingCore.RoslynRewriters;
using TracingCore.TreeRewriters;

namespace OptLocal.Areas.Default.Controllers
{
    [Area("Default")]
    public class TraceApiController : Controller
    {
        private const string CompilationName = "opt-compilation";
        private readonly InstrumentationConfig _instrumentationConfig;

        public TraceApiController(InstrumentationConfig instrumentationConfig)
        {
            _instrumentationConfig = instrumentationConfig;
            // Refactor in the future
            PyTutorStepMapper.RegisterConfig(_instrumentationConfig);
        }

        [HttpGet, Route("/api/getcsharptrace")]
        public string GetCSharpTrace(
            // [FromQuery] string user_script,
            // [FromQuery] string options_json,
            // [FromQuery] string raw_input_json
            )
        {
            // var inputs = raw_input_json == null
            //     ? new List<string>()
            //     : JArray.Parse(raw_input_json).ToObject<List<string>>();
            // var sourceRewriter = new SourceCodeRewriter(new ExpressionGenerator(), _instrumentationConfig);
            // var optBackend = new OptBackend(user_script, inputs, sourceRewriter, _instrumentationConfig);
            //
            // var compilationResult = optBackend.Compile(CompilationName, true);
            // var pyTutorData = optBackend.Trace(compilationResult.Root, compilationResult);
            //
            // return PyTutorDataMapper.ToJson(pyTutorData);

            return "SRANJE";
        }
        
        [HttpGet, Route("/api/statuscheck")]
        public string GetCSharpTrace(int a)
        {
            return _instrumentationConfig.Property.BackupNamePrefix;
        }
    }
}