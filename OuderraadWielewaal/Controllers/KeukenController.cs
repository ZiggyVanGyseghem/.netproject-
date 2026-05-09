using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OuderraadWielewaal.Data;
using OuderraadWielewaal.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OuderraadWielewaal.Controllers
{
    // Alleen toegankelijk voor Keuken-personeel en Administrators
    [Authorize(Roles = "Keuken,Administrator")]
    public class KeukenController : Controller
    {
        private readonly AppDbContext _context;

        public KeukenController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var openBestellingen = await _context.Bestellingen
                .Include(b => b.Gebruiker)
                    .ThenInclude(g => g.Tafeltoewijzingen)
                        .ThenInclude(tt => tt.Tafel)
                .Include(b => b.Bestellijnen)
                    .ThenInclude(bl => bl.Product)
                        .ThenInclude(p => p.Productdetails)
                .Where(b => b.Status == BestelStatus.InDeWachtrij)
                .OrderBy(b => b.TijdstipBesteld)
                .ToListAsync();

            return View(openBestellingen);
        }

        [HttpPost]
        public async Task<IActionResult> MarkeerAlsKlaar(int id)
        {
            var bestelling = await _context.Bestellingen.FindAsync(id);
            if (bestelling != null)
            {
                bestelling.Status = BestelStatus.KlaarVoorOphalen;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}