using Microsoft.AspNetCore.Mvc;

namespace OuderraadWielewaal.Controllers
{
    public class MainController : Controller
    {
        // This action returns the starter page view
        public IActionResult Index()
        {
            return View();
        }
    }
}
