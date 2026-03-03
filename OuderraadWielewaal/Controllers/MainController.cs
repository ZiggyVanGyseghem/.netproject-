using Microsoft.AspNetCore.Mvc;

namespace OuderraadWielewaal.Controllers
{
    public class MainController : Controller
    {
        // Loads the Start Page
        public IActionResult Index()
        {
            return View();
        }

        // Loads the Menu Page
        public IActionResult Menu()
        {
            return View();
        }
    }
}