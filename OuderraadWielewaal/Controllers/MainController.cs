using Microsoft.AspNetCore.Mvc;
using OuderraadWielewaal.Data;
using System.Linq;
using OuderraadWielewaal.Extensions;
using OuderraadWielewaal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Mollie.Api.Client;
using Mollie.Api.Models.Payment.Request;

namespace OuderraadWielewaal.Controllers
{
    public class MainController : Controller
    {
        private readonly AppDbContext _context;

        public MainController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Menu(int tafelId = 1)
        {
            var producten = _context.Productdetails.ToList();
            ViewBag.TafelNummer = tafelId;
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
                bestaandItem.Aantal += aantal;
            }
            else
            {
                mandje.Add(new WinkelmandItem
                {
                    ProductId = product.ProductId,
                    Naam = product.Naam,
                    Prijs = product.Prijs,
                    Aantal = aantal
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
                var item = mandje.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    mandje.Remove(item);
                    HttpContext.Session.SetObjectAsJson("Winkelmandje", mandje);
                }
            }

            return RedirectToAction("Winkelmandje", new { tafelId = tafelId });
        }

        public IActionResult Winkelmandje(int tafelId)
        {
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("Winkelmandje") ?? new List<WinkelmandItem>();
            ViewBag.TafelNummer = tafelId;
            return View(mandje);
        }

        // --- SCHONE AFREKENEN CODE ---
        [HttpPost]
        public async Task<IActionResult> Afrekenen(int tafelId)
        {
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("Winkelmandje");
            if (mandje == null || !mandje.Any()) return RedirectToAction("Winkelmandje", new { tafelId = tafelId });

            // 1. Bereken het totaalbedrag voor Mollie
            var productIds = mandje.Select(i => i.ProductId).ToList();
            var productenUitDb = _context.Productdetails.Where(p => productIds.Contains(p.ProductId)).ToList();

            double totaalPrijs = 0;
            foreach (var item in mandje)
            {
                var product = productenUitDb.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product != null) totaalPrijs += (product.Prijs * item.Aantal);
            }

            var nieuweBestelling = new Bestelling
            {
                GebruikerId = 1, // Tijdelijk
                TijdstipBesteld = DateTime.Now,
                Status = BestelStatus.InDeWachtrij,
                BetaalStatus = BetaalStatus.Open
            };

            foreach (var item in mandje)
            {
                nieuweBestelling.Bestellijnen.Add(new Bestellijn { ProductId = item.ProductId, Hoeveelheid = item.Aantal });
            }

            _context.Bestellingen.Add(nieuweBestelling);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Winkelmandje");

            // 2. MAAK DE MOLLIE BETALING AAN
            var mollieClient = new PaymentClient("test_ASHM9vDq92bmmBMaHSdwTvd5Q3Tms7");

            var paymentRequest = new PaymentRequest
            {
                Amount = new Mollie.Api.Models.Amount(Mollie.Api.Models.Currency.EUR, totaalPrijs.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)),
                Description = $"ORW Bestelling #{nieuweBestelling.Id} - Tafel {tafelId}",

                // Waar moet de klant heen na het betalen? Naar de bedankt-pagina!
                RedirectUrl = $"http://localhost:5012/Main/Bedankt?tafelId={tafelId}&bestellingId={nieuweBestelling.Id}",

                // Waar moet Mollie stiekem een berichtje naartoe sturen als de betaling is gelukt?
                WebhookUrl = "https://jouwwebsite.nl/api/mollie/webhook",

                Metadata = nieuweBestelling.Id.ToString()
            };

            var paymentResponse = await mollieClient.CreatePaymentAsync(paymentRequest);

            // 3. STUUR DE KLANT NAAR HET BETAALSCHERM!
            return Redirect(paymentResponse.Links.Checkout.Href);
        }

        // We maken hem 'async Task' omdat we de database gaan updaten
        public async Task<IActionResult> Bedankt(int tafelId, int bestellingId)
        {
            // 1. Zoek de bestelling op in de database
            var bestelling = await _context.Bestellingen.FindAsync(bestellingId);

            // 2. Als we hem vinden, en hij staat nog op Open...
            if (bestelling != null && bestelling.BetaalStatus == OuderraadWielewaal.Models.BetaalStatus.Open)
            {
                // ...dan zetten we hem nu op Betaald!
                bestelling.BetaalStatus = OuderraadWielewaal.Models.BetaalStatus.Betaald;
                await _context.SaveChangesAsync();
            }

            ViewBag.TafelNummer = tafelId;
            ViewBag.BestellingId = bestellingId;
            return View();
        }
    }
}