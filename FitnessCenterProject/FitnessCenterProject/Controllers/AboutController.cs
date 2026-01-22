using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterProject.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
