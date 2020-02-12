using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TracingCore;
using TracingCore.JsonMappers;

namespace OptLocal.Areas.Default.Controllers
{
    public class Data
    {
        public string UserScript { get; set; }
        public string OptionsJson { get; set; }
    }

    public class MyCustomModel
    {
        public string DeviceName { get; set; }
    }

    public class MyViewModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var jsonString = bindingContext.ActionContext.HttpContext.Request.Query["query"];
            MyCustomModel result = JsonConvert.DeserializeObject<MyCustomModel>(jsonString);

            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }
    }

    public class MyViewModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(MyCustomModel))
                return new MyViewModelBinder();

            return null;
        }
    }

    [Area("Default")]
    public class TraceApiController : Controller
    {
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

            var compilationResult = optBackend.Compile();
            var pyTutorData = optBackend.Trace(compilationResult.Root, compilationResult);

            return PyTutorDataMapper.ToJson(pyTutorData);
        }
    }
}