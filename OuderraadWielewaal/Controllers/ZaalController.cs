using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OuderraadWielewaal.Data;
using OuderraadWielewaal.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OuderraadWielewaal.Controllers
{
    // Alleen toegankelijk voor de bediening (Zaal) en Administrators
    [Authorize(Roles = "Zaal,Administrator")]
    public class ZaalController : Controller
    {
        private readonly AppDbContext _context;

        public ZaalController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Haal alle bestellingen op die klaar staan om geserveerd te worden!
            var klaarBestellingen = await _context.Bestellingen
                .Include(b => b.Gebruiker)
                .Include(b => b.Bestellijnen)
                    .ThenInclude(bl => bl.Product)
                        .ThenInclude(p => p.Productdetails)
                .Where(b => b.Status == BestelStatus.KlaarVoorOphalen) // <-- Het magische filter
                .OrderBy(b => b.TijdstipBesteld)
                .ToListAsync();

            return View(klaarBestellingen);
        }

        [HttpPost]
        public async Task<IActionResult> MarkeerAlsAfgeleverd(int id)
        {
            var bestelling = await _context.Bestellingen.FindAsync(id);
            if (bestelling != null)
            {
                // De bestelling is naar de tafel gebracht. We zijn helemaal klaar!
                bestelling.Status = BestelStatus.Afgeleverd;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}