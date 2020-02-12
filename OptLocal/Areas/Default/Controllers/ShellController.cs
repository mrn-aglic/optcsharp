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
    }
}