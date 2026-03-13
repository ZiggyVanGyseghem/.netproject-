using Microsoft.AspNetCore.Mvc;
using OuderraadWielewaal.Data;
using System.Linq;

namespace OuderraadWielewaal.Controllers
{
    public class MainController : Controller
    {
        private readonly AppDbContext _context;

        // Dit is "Dependency Injection". .NET geeft automatisch de database door aan deze controller.
        public MainController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // We accepteren nu een tafelId via de URL (standaard op 1 als er niets is ingevuld)
        public IActionResult Menu(int tafelId = 1)
        {
            // Haal alle producten uit de database
            var producten = _context.Productdetails.ToList();

            // Sla het tafelnummer op zodat de HTML pagina het kan lezen
            ViewBag.TafelNummer = tafelId;

            // Geef de lijst met producten mee aan de View
            return View(producten);
        }
    }
}