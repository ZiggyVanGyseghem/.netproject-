using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OuderraadWielewaal.Data;
using OuderraadWielewaal.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OuderraadWielewaal.Controllers
{
    // 1. DE UITSMIJTER: Alleen accounts met de rol "Bar" (of "Administrator") mogen hier naar binnen!
    [Authorize(Roles = "Bar,Administrator")]
    public class BarController : Controller
    {
        private readonly AppDbContext _context;

        public BarController(AppDbContext context)
        {
            _context = context;
        }

        // 2. HET DASHBOARD SCHERM
        public async Task<IActionResult> Index()
        {
            // Haal alle openstaande bestellingen op uit de database
            var openBestellingen = await _context.Bestellingen
                .Include(b => b.Gebruiker) // Wie heeft het besteld?
                .Include(b => b.Bestellijnen) // Welke regels zitten er in de bestelling?
                    .ThenInclude(bl => bl.Product) // Welk product hoort bij die regel?
                        .ThenInclude(p => p.Productdetails) // Wat is de naam en het type van het product?
                .Where(b => b.Status == BestelStatus.InDeWachtrij) // Alleen bestellingen die in de wachtrij staan
                .OrderBy(b => b.TijdstipBesteld) // De oudste bestellingen bovenaan (wie het eerst komt...)
                .ToListAsync();

            return View(openBestellingen);
        }

        // 3. DE KNOP: BESTELLING KLAARZETTEN
        [HttpPost]
        public async Task<IActionResult> MarkeerAlsKlaar(int id)
        {
            var bestelling = await _context.Bestellingen.FindAsync(id);
            if (bestelling != null)
            {
                // Zet de status een stapje verder!
                bestelling.Status = BestelStatus.KlaarVoorOphalen;
                await _context.SaveChangesAsync();
            }

            // Ververs de pagina zodat de bestelling uit de lijst verdwijnt
            return RedirectToAction(nameof(Index));
        }
    }
}