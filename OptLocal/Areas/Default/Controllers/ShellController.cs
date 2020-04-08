using Microsoft.AspNetCore.Mvc;

namespace OptLocal.Areas.Default.Controllers
{
    [Area("Default")]
    public class ShellController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult JavaScript()
        {
            return View();
        }

        public IActionResult Monaco()
        {
            return View();
        }
    }
}