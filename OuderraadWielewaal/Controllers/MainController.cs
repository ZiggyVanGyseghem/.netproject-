using Microsoft.AspNetCore.Mvc;
using OuderraadWielewaal.Data;
using System.Linq;
using OuderraadWielewaal.Extensions;
using OuderraadWielewaal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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

        [HttpPost]
        public IActionResult VoegToeAanMandje(int productId, int tafelId, int aantal = 1)
        {
            var product = _context.Productdetails.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) return NotFound();

            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("Winkelmandje") ?? new List<WinkelmandItem>();

            var bestaandItem = mandje.FirstOrDefault(i => i.ProductId == productId);
            if (bestaandItem != null)
            {
                // Tel het gekozen aantal op bij wat er al in het mandje zat!
                bestaandItem.Aantal += aantal;
            }
            else
            {
                mandje.Add(new WinkelmandItem
                {
                    ProductId = product.ProductId,
                    Naam = product.Naam,
                    Prijs = product.Prijs,
                    Aantal = aantal // Gebruik het gekozen aantal!
                });
            }

            HttpContext.Session.SetObjectAsJson("Winkelmandje", mandje);
            return RedirectToAction("Menu", new { tafelId = tafelId });
        }

        public IActionResult VerwijderUitMandje(int productId, int tafelId)
        {
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("Winkelmandje");

            if (mandje != null)
            {
                // Zoek het product en verwijder het uit de lijst
                var item = mandje.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    mandje.Remove(item);
                    // Sla de geüpdatete lijst weer op
                    HttpContext.Session.SetObjectAsJson("Winkelmandje", mandje);
                }
            }

            // Stuur terug naar het winkelmandje
            return RedirectToAction("Winkelmandje", new { tafelId = tafelId });
        }

        // Laadt de Winkelmandje pagina
        public IActionResult Winkelmandje(int tafelId)
        {
            // Haal het mandje uit het geheugen (of maak een lege lijst als hij er niet is)
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("Winkelmandje") ?? new List<WinkelmandItem>();

            // Stuur het tafelnummer weer mee
            ViewBag.TafelNummer = tafelId;

            // Geef de lijst met items aan de pagina
            return View(mandje);
        }

        // --- NIEUWE AFREKENEN CODE (Nu netjes BINNEN de klasse!) ---

        [HttpPost]
        public async Task<IActionResult> Afrekenen(int tafelId)
        {
            // 1. Haal het mandje uit de sessie
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("Winkelmandje");

            if (mandje == null || !mandje.Any())
            {
                return RedirectToAction("Winkelmandje", new { tafelId = tafelId });
            }

            // 2. Zoek op welke items drankjes zijn en welke snacks (via de database)
            var productIds = mandje.Select(i => i.ProductId).ToList();
            var productenUitDb = _context.Productdetails.Where(p => productIds.Contains(p.ProductId)).ToList();

            var drankjes = mandje.Where(i => productenUitDb.Any(p => p.ProductId == i.ProductId && p.ProductType == ProductType.Drank)).ToList();
            var snacks = mandje.Where(i => productenUitDb.Any(p => p.ProductId == i.ProductId && p.ProductType == ProductType.Versnapering)).ToList();

            int laatsteBestelId = 0;

            // 3. Maak een losse bestelling (bonnetje) voor de BAR
            if (drankjes.Any())
            {
                var drankBestelling = new Bestelling
                {
                    GebruikerId = 1, // Tijdelijk
                    TijdstipBesteld = DateTime.Now,
                    Status = BestelStatus.InDeWachtrij,
                    BetaalStatus = BetaalStatus.Open
                };

                foreach (var item in drankjes)
                {
                    drankBestelling.Bestellijnen.Add(new Bestellijn { ProductId = item.ProductId, Hoeveelheid = item.Aantal });
                }

                _context.Bestellingen.Add(drankBestelling);
                await _context.SaveChangesAsync();
                laatsteBestelId = drankBestelling.Id;
            }

            // 4. Maak een losse bestelling (bonnetje) voor de KEUKEN
            if (snacks.Any())
            {
                var snackBestelling = new Bestelling
                {
                    GebruikerId = 1, // Tijdelijk
                    TijdstipBesteld = DateTime.Now,
                    Status = BestelStatus.InDeWachtrij,
                    BetaalStatus = BetaalStatus.Open
                };

                foreach (var item in snacks)
                {
                    snackBestelling.Bestellijnen.Add(new Bestellijn { ProductId = item.ProductId, Hoeveelheid = item.Aantal });
                }

                _context.Bestellingen.Add(snackBestelling);
                await _context.SaveChangesAsync();
                laatsteBestelId = snackBestelling.Id;
            }

            // 5. Maak het winkelmandje leeg
            HttpContext.Session.Remove("Winkelmandje");

            // 6. Stuur door naar bedankt pagina
            return RedirectToAction("Bedankt", new { tafelId = tafelId, bestellingId = laatsteBestelId });
        }
        // De bedank-pagina die getoond wordt na het afrekenen
        public IActionResult Bedankt(int tafelId, int bestellingId)
        {
            ViewBag.TafelNummer = tafelId;
            ViewBag.BestellingId = bestellingId;
            return View();
        }
    } 
}