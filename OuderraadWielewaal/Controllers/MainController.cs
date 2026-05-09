using Microsoft.AspNetCore.Mvc;
using OuderraadWielewaal.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using OuderraadWielewaal.Extensions;
using OuderraadWielewaal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Mollie.Api.Client;
using Mollie.Api.Models.Payment.Request;
using Microsoft.AspNetCore.Authorization; // NIEUW: Voor de beveiliging
using Microsoft.AspNetCore.Identity;      // NIEUW: Voor het ophalen van de gebruiker

namespace OuderraadWielewaal.Controllers
{
    // Zorgt ervoor dat je voor het hele menu ingelogd moet zijn!
    [Authorize]
    public class MainController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Gebruiker> _userManager; // NIEUW

        // Voeg de UserManager toe aan de opstart-methode
        public MainController(AppDbContext context, UserManager<Gebruiker> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // De homepagina mag nog wel door iedereen bekeken worden
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        // De bezoeker scant de QR-code en komt hier terecht (geen inlogscherm!)
        [AllowAnonymous]
        public async Task<IActionResult> OpenTafel(string code)
        {
            if (string.IsNullOrEmpty(code))
                return View("TafelFout", "Ongeldige QR-code.");

            var tafel = await _context.Tafels.FirstOrDefaultAsync(t => t.UniekeCode == code);

            if (tafel == null)
                return View("TafelFout", (object)"Deze QR-code is onbekend. Vraag de bediening om hulp.");

            if (!tafel.Actief)
                return View("TafelFout", (object)"Deze tafel is momenteel niet actief. Vraag de bediening om hulp.");

            // Sla de tafelcode op in de sessie zodat we weten voor welke tafel we bestellen
            HttpContext.Session.SetString("TafelCode", code);
            HttpContext.Session.SetInt32("TafelId", tafel.Id);
            HttpContext.Session.SetInt32("TafelNummer", tafel.Nummer);

            return RedirectToAction("GastMenu");
        }

        // Het gasten-menu (zonder login, via sessie)
        [AllowAnonymous]
        public async Task<IActionResult> GastMenu()
        {
            var tafelCode = HttpContext.Session.GetString("TafelCode");
            var tafelId = HttpContext.Session.GetInt32("TafelId");

            if (string.IsNullOrEmpty(tafelCode) || !tafelId.HasValue)
                return RedirectToAction("Index");

            // CHECK: Is de tafel nog steeds actief?
            var tafel = await _context.Tafels.FindAsync(tafelId.Value);
            if (tafel == null || !tafel.Actief)
            {
                HttpContext.Session.Clear();
                return View("TafelFout", (object)"Deze tafel is gedeactiveerd door de beheerder.");
            }

            var tafelNummer = HttpContext.Session.GetInt32("TafelNummer");
            ViewBag.TafelNummer = tafelNummer;

            var producten = await _context.Producten
                .Include(p => p.Productdetails)
                .ToListAsync();

            var dranken = producten
                .Where(p => p.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().ProductType == ProductType.Drank)
                .ToList();
            var snacks = producten
                .Where(p => p.Productdetails.OrderByDescending(pd => pd.Tijdstip).First().ProductType == ProductType.Versnapering)
                .ToList();

            ViewBag.Dranken = dranken;
            ViewBag.Snacks = snacks;

            return View();
        }

        public async Task<IActionResult> Menu()
        {
            var producten = _context.Productdetails.ToList();

            // Haal de momenteel ingelogde tafel/gebruiker op
            var ingelogdeGebruiker = await _userManager.GetUserAsync(User);

            // Geef het ID van de ingelogde tafel door aan de view
            ViewBag.TafelNummer = ingelogdeGebruiker?.Id ?? 0;

            return View(producten);
        }

        [HttpPost]
        public async Task<IActionResult> VoegToeAanMandje(int productId, int aantal = 1)
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
                mandje.Add(new WinkelmandItem { ProductId = product.ProductId, Naam = product.Naam, Prijs = product.Prijs, Aantal = aantal });
            }

            HttpContext.Session.SetObjectAsJson("Winkelmandje", mandje);
            return RedirectToAction("Menu");
        }

        public IActionResult VerwijderUitMandje(int productId)
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
            return RedirectToAction("Winkelmandje");
        }

        public async Task<IActionResult> Winkelmandje()
        {
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("Winkelmandje") ?? new List<WinkelmandItem>();

            var ingelogdeGebruiker = await _userManager.GetUserAsync(User);
            ViewBag.TafelNummer = ingelogdeGebruiker?.Id ?? 0;

            return View(mandje);
        }

        // --- GAST FLOW (ZONDER INLOGGEN) ---

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GastVoegToe(int productId, int aantal = 1)
        {
            var tafelId = HttpContext.Session.GetInt32("TafelId");
            var tafel = await _context.Tafels.FindAsync(tafelId);
            if (tafel == null || !tafel.Actief) return RedirectToAction("Index");

            var product = await _context.Productdetails.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null) return NotFound();

            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("GastMandje") ?? new List<WinkelmandItem>();
            var bestaandItem = mandje.FirstOrDefault(i => i.ProductId == productId);

            if (bestaandItem != null)
            {
                bestaandItem.Aantal += aantal;
            }
            else
            {
                mandje.Add(new WinkelmandItem { ProductId = product.ProductId, Naam = product.Naam, Prijs = product.Prijs, Aantal = aantal });
            }

            HttpContext.Session.SetObjectAsJson("GastMandje", mandje);
            return RedirectToAction("GastMenu");
        }

        [AllowAnonymous]
        public IActionResult GastWinkelmandje()
        {
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("GastMandje") ?? new List<WinkelmandItem>();
            ViewBag.TafelNummer = HttpContext.Session.GetInt32("TafelNummer") ?? 0;
            return View(mandje);
        }

        [AllowAnonymous]
        public IActionResult GastVerwijder(int productId)
        {
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("GastMandje");
            if (mandje != null)
            {
                var item = mandje.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    mandje.Remove(item);
                    HttpContext.Session.SetObjectAsJson("GastMandje", mandje);
                }
            }
            return RedirectToAction("GastWinkelmandje");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GastAfrekenen()
        {
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("GastMandje");
            var tafelId = HttpContext.Session.GetInt32("TafelId");

            if (mandje == null || !mandje.Any() || !tafelId.HasValue) 
                return RedirectToAction("GastWinkelmandje");

            // FINAL CHECK: Is de tafel nog actief voor we betaling starten?
            var tafel = await _context.Tafels.FindAsync(tafelId.Value);
            if (tafel == null || !tafel.Actief)
            {
                HttpContext.Session.Clear();
                return View("TafelFout", (object)"Deze tafel is zojuist gedeactiveerd. Je kunt geen bestelling meer plaatsen.");
            }

            // ACHTERGROND TRUC: Maak onzichtbare bezoeker
            var uniekeBezoekerNaam = $"Bezoeker_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var nieuweBezoeker = new Gebruiker
            {
                UserName = uniekeBezoekerNaam,
                Naam = "Bezoeker",
                TijdstipGeactiveerd = DateTime.Now
            };

            // Maak de gebruiker aan in Identity (zonder wachtwoord voor nu, of een random een)
            var createResult = await _userManager.CreateAsync(nieuweBezoeker, Guid.NewGuid().ToString() + "A1!");
            if (!createResult.Succeeded) return RedirectToAction("GastMenu");

            // Koppel aan de tafel via Tafeltoewijzing
            var toewijzing = new Tafeltoewijzing
            {
                GebruikerId = nieuweBezoeker.Id,
                TafelId = tafelId.Value,
                TijdstipToegewezen = DateTime.Now
            };
            _context.Tafeltoewijzingen.Add(toewijzing);

            // Bereken totaal en maak bestelling
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
                GebruikerId = nieuweBezoeker.Id,
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
            
            // Sessie opschonen (mandje leeg, maar bezoeker ID even onthouden voor de tracker)
            HttpContext.Session.Remove("GastMandje");
            HttpContext.Session.SetInt32("HuidigeBestellingId", nieuweBestelling.Id);

            // Mollie (Test API)
            var mollieClient = new PaymentClient("test_ASHM9vDq92bmmBMaHSdwTvd5Q3Tms7");
            var paymentRequest = new PaymentRequest
            {
                Amount = new Mollie.Api.Models.Amount(Mollie.Api.Models.Currency.EUR, totaalPrijs.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)),
                Description = $"ORW Bestelling #{nieuweBestelling.Id} - Tafel {HttpContext.Session.GetInt32("TafelNummer")}",
                RedirectUrl = Url.Action("GastBedankt", "Main", new { bestellingId = nieuweBestelling.Id }, Request.Scheme),
                WebhookUrl = Url.Action("Webhook", "Mollie", null, Request.Scheme),
                Metadata = nieuweBestelling.Id.ToString()
            };

            var paymentResponse = await mollieClient.CreatePaymentAsync(paymentRequest);
            return Redirect(paymentResponse.Links.Checkout.Href);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GastBedankt(int bestellingId)
        {
            var bestelling = await _context.Bestellingen
                .Include(b => b.Bestellijnen)
                    .ThenInclude(bl => bl.Product)
                        .ThenInclude(p => p.Productdetails)
                .FirstOrDefaultAsync(b => b.Id == bestellingId);

            // LOCALHOST FIX
            if (bestelling != null && bestelling.BetaalStatus == BetaalStatus.Open)
            {
                bestelling.BetaalStatus = BetaalStatus.Betaald;
                await _context.SaveChangesAsync();
            }

            ViewBag.TafelNummer = HttpContext.Session.GetInt32("TafelNummer") ?? 0;
            ViewBag.BestellingId = bestellingId;

            return View("Bedankt", bestelling); // Gebruik de bestaande mooie Bedankt view
        }

        [HttpPost]
        public async Task<IActionResult> Afrekenen()
        {
            var mandje = HttpContext.Session.GetObjectFromJson<List<WinkelmandItem>>("Winkelmandje");
            if (mandje == null || !mandje.Any()) return RedirectToAction("Winkelmandje");

            // WIE IS ER INGELOGD?
            var ingelogdeGebruiker = await _userManager.GetUserAsync(User);
            if (ingelogdeGebruiker == null) return Challenge(); // Stuur naar login als het toch misgaat

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
                // DE MAGIE: We gebruiken nu het ECHTE ID in plaats van de hardcoded 1!
                GebruikerId = ingelogdeGebruiker.Id,
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

            var mollieClient = new PaymentClient("test_ASHM9vDq92bmmBMaHSdwTvd5Q3Tms7");

            var paymentRequest = new PaymentRequest
            {
                Amount = new Mollie.Api.Models.Amount(Mollie.Api.Models.Currency.EUR, totaalPrijs.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)),
                Description = $"ORW Bestelling #{nieuweBestelling.Id} - Tafel {ingelogdeGebruiker.Id}",
                RedirectUrl = Url.Action("Bedankt", "Main", new { bestellingId = nieuweBestelling.Id }, Request.Scheme),
                WebhookUrl = Url.Action("Webhook", "Mollie", null, Request.Scheme),
                Metadata = nieuweBestelling.Id.ToString()
            };

            var paymentResponse = await mollieClient.CreatePaymentAsync(paymentRequest);
            return Redirect(paymentResponse.Links.Checkout.Href);
        }

        public async Task<IActionResult> Bedankt(int bestellingId)
        {
            var bestelling = await _context.Bestellingen
                .Include(b => b.Bestellijnen)
                    .ThenInclude(bl => bl.Product)
                        .ThenInclude(p => p.Productdetails)
                .FirstOrDefaultAsync(b => b.Id == bestellingId);

            // --- LOCALHOST FIX VOOR LOKAAL TESTEN ---
            // Mollie kan je localhost webhook niet bereiken. Dus we doen het hier tijdelijk voor lokaal testen.
            if (bestelling != null && bestelling.BetaalStatus == BetaalStatus.Open)
            {
                bestelling.BetaalStatus = BetaalStatus.Betaald;

                // --- SPLITS DE BESTELLING ---
                var dranken = bestelling.Bestellijnen
                    .Where(bl => bl.Product.Productdetails.Any(pd => pd.ProductType == ProductType.Drank))
                    .ToList();
                var snacks = bestelling.Bestellijnen
                    .Where(bl => bl.Product.Productdetails.Any(pd => pd.ProductType == ProductType.Versnapering))
                    .ToList();

                if (dranken.Any() && snacks.Any())
                {
                    // Er zijn zowel dranken als snacks. We maken een nieuwe bestelling voor de snacks.
                    var snacksBestelling = new Bestelling
                    {
                        GebruikerId = bestelling.GebruikerId,
                        TijdstipBesteld = bestelling.TijdstipBesteld,
                        Status = bestelling.Status,
                        BetaalStatus = bestelling.BetaalStatus,
                        MolliePaymentId = bestelling.MolliePaymentId
                    };

                    foreach (var snack in snacks)
                    {
                        snacksBestelling.Bestellijnen.Add(new Bestellijn
                        {
                            ProductId = snack.ProductId,
                            Hoeveelheid = snack.Hoeveelheid
                        });
                        bestelling.Bestellijnen.Remove(snack);
                    }

                    _context.Bestellingen.Add(snacksBestelling);
                }

                await _context.SaveChangesAsync();
            }

            var ingelogdeGebruiker = await _userManager.GetUserAsync(User);
            ViewBag.TafelNummer = ingelogdeGebruiker?.UserName ?? "Onbekend";
            ViewBag.BestellingId = bestellingId;

            return View(bestelling);
        }

        [HttpGet]
        public async Task<IActionResult> BestelStatusTracker(int id)
        {
            // Ophalen van de originele bestelling OF een afgesplitste bestelling
            // Omdat de bestelling gesplitst wordt in bar en keuken, kan de ID zijn veranderd
            // Maar als we de originele ophalen (die dan bv alleen drank bevat) is de status hetzelfde (beide gaan naar InDeWachtrij etc).
            // Voor de simpelheid en robuustheid zoeken we de bestellingen van deze gebruiker in het laatste uur met dezelfde TijdstipBesteld.
            var bestelling = await _context.Bestellingen.FindAsync(id);
            if (bestelling == null) return NotFound();

            return Json(new { status = bestelling.Status.ToString() });
        }
    }
}