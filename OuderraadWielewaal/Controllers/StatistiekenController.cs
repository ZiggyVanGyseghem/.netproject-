using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OuderraadWielewaal.Data;
using OuderraadWielewaal.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OuderraadWielewaal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatistiekenController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatistiekenController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("producten")]
        public async Task<IActionResult> GetProductStatistieken()
        {
            var bestellijnen = await _context.Bestellijnen
                .Include(bl => bl.Product)
                    .ThenInclude(p => p.Productdetails)
                .Include(bl => bl.Bestelling)
                .Where(bl => bl.Bestelling.BetaalStatus == BetaalStatus.Betaald)
                .ToListAsync();

            var drankenGroep = bestellijnen
                .Where(bl => bl.Product.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().ProductType == ProductType.Drank)
                .GroupBy(bl => bl.Product.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().Naam)
                .Select(g => new { Naam = g.Key, Aantal = g.Sum(bl => bl.Hoeveelheid) })
                .ToList();

            var snacksGroep = bestellijnen
                .Where(bl => bl.Product.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().ProductType == ProductType.Versnapering)
                .GroupBy(bl => bl.Product.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().Naam)
                .Select(g => new { Naam = g.Key, Aantal = g.Sum(bl => bl.Hoeveelheid) })
                .ToList();

            return Ok(new
            {
                MeestBesteldeDrank = drankenGroep.OrderByDescending(d => d.Aantal).FirstOrDefault(),
                MinstBesteldeDrank = drankenGroep.OrderBy(d => d.Aantal).FirstOrDefault(),
                MeestBesteldeVersnapering = snacksGroep.OrderByDescending(s => s.Aantal).FirstOrDefault(),
                MinstBesteldeVersnapering = snacksGroep.OrderBy(s => s.Aantal).FirstOrDefault()
            });
        }

        [HttpGet("tafels")]
        public async Task<IActionResult> GetTafelStatistieken()
        {
            var bestellijnen = await _context.Bestellijnen
                .Include(bl => bl.Product)
                    .ThenInclude(p => p.Productdetails)
                .Include(bl => bl.Bestelling)
                    .ThenInclude(b => b.Gebruiker)
                        .ThenInclude(u => u.Tafeltoewijzingen)
                            .ThenInclude(tt => tt.Tafel)
                .Where(bl => bl.Bestelling.BetaalStatus == BetaalStatus.Betaald)
                .ToListAsync();

            var tafelUitgaven = bestellijnen
                .GroupBy(bl => {
                    var tafel = bl.Bestelling.Gebruiker.Tafeltoewijzingen.FirstOrDefault()?.Tafel;
                    return tafel != null ? $"Tafel {tafel.Nummer}" : bl.Bestelling.Gebruiker.UserName;
                })
                .Select(g => new
                {
                    Tafel = g.Key,
                    UitgavenDrank = g.Where(bl => bl.Product.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().ProductType == ProductType.Drank)
                                     .Sum(bl => bl.Hoeveelheid * bl.Product.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().Prijs),
                    UitgavenSnacks = g.Where(bl => bl.Product.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().ProductType == ProductType.Versnapering)
                                      .Sum(bl => bl.Hoeveelheid * bl.Product.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().Prijs)
                })
                .ToList();

            var topDrankTafel = tafelUitgaven.OrderByDescending(t => t.UitgavenDrank).FirstOrDefault();
            var topSnacksTafel = tafelUitgaven.OrderByDescending(t => t.UitgavenSnacks).FirstOrDefault();

            return Ok(new
            {
                TafelMeesteUitgavenDrank = topDrankTafel,
                TafelMeesteUitgavenVersnapering = topSnacksTafel
            });
        }
    }
}
