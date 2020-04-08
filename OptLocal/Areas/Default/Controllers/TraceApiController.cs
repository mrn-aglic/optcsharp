using System.Collections.Generic;
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
        public JObject GetCSharpTrace(
            [FromQuery] string user_script,
            [FromQuery] string options_json,
            [FromQuery] string raw_input_json
        )
        {
            var inputs = raw_input_json == null
                ? new List<string>()
                : JArray.Parse(raw_input_json).ToObject<List<string>>();

            var optBackend = new OptBackend(user_script, inputs);

            var scriptCompilation = optBackend.Compile("user-code");

            if (!scriptCompilation.Success)
            {
                return PyTutorDataMapper.ToJson(optBackend.ReportCompilationError(scriptCompilation));
            }

            var scriptSemanticModel = scriptCompilation.GetSemanticModel();

            var sourceRewriter =
                new SourceCodeRewriter(new ExpressionGenerator(scriptSemanticModel), _instrumentationConfig);
            var instrumentationManager = new InstrumentationManager(sourceRewriter);

            var compilationResult = optBackend.Compile(CompilationName, instrumentationManager);

            var pyTutorData = optBackend.Trace(compilationResult);
            return PyTutorDataMapper.ToJson(pyTutorData);
        }

        [HttpPost, Route("/api/getcsharptrace")]
        public JObject GetCSharpTrace(
            [FromBody] JObject json
        )
        {
            var rawInput = json["rawInputJson"].ToObject<List<string>>();
            var userCode = json["code"].ToObject<string>();

            var optBackend = new OptBackend(userCode, rawInput);
            var originalSourceCompilation = optBackend.Compile("user-code");
            var userSemanticModel =
                originalSourceCompilation.Compilation.GetSemanticModel(optBackend.UserSyntaxTree);
            var sourceRewriter =
                new SourceCodeRewriter(new ExpressionGenerator(userSemanticModel), _instrumentationConfig);
            var instrumentationManager = new InstrumentationManager(sourceRewriter);

            var compilationResult = optBackend.Compile(CompilationName, instrumentationManager);

            if (!compilationResult.Success)
            {
                return PyTutorDataMapper.ToJson(optBackend.ReportCompilationError(compilationResult));
            }

            var pyTutorData = optBackend.Trace(compilationResult);
            return PyTutorDataMapper.ToJson(pyTutorData);
        }

        [HttpGet, Route("/api/statuscheck")]
        public JObject GetCSharpTrace(int a)
        {
            return JObject.FromObject(_instrumentationConfig.Property.BackupNamePrefix);
        }
    }
}