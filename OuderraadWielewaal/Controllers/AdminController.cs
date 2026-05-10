using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OuderraadWielewaal.Models;
using OuderraadWielewaal.ViewModels;
using OuderraadWielewaal.Data;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace OuderraadWielewaal.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<Gebruiker> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<Gebruiker> userManager, RoleManager<Rol> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // 1. Het Admin Dashboard (Keuzescherm met de grote knoppen)
        public IActionResult Index()
        {
            return View();
        }

        // 2. Overzicht van alle gebruikers (Dit is de pagina die een 404 gaf!)
        public async Task<IActionResult> Gebruikers()
        {
            var alleGebruikers = await _userManager.Users.ToListAsync();
            return View(alleGebruikers);
        }

        // 3. Haalt het grote overzicht op van ALLE bestellingen voor de beheerder
        public async Task<IActionResult> Bestellingen()
        {
            var alleBestellingen = await _context.Bestellingen
                .Include(b => b.Gebruiker)
                    .ThenInclude(u => u.Tafeltoewijzingen)
                        .ThenInclude(tt => tt.Tafel)
                .Include(b => b.Bestellijnen)
                    .ThenInclude(bl => bl.Product)
                        .ThenInclude(p => p.Productdetails)
                .OrderByDescending(b => b.TijdstipBesteld) // Nieuwste bovenaan
                .ToListAsync();

            return View(alleBestellingen);
        }

        // 3b. Detailpagina voor 1 specifieke bestelling
        public async Task<IActionResult> BestellingDetail(int id)
        {
            var bestelling = await _context.Bestellingen
                .Include(b => b.Gebruiker)
                .Include(b => b.Bestellijnen)
                    .ThenInclude(bl => bl.Product)
                        .ThenInclude(p => p.Productdetails)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bestelling == null) return NotFound();

            return View(bestelling);
        }

        // 4. Laat het formulier zien om een gebruiker te maken (GET)
        [HttpGet]
        public async Task<IActionResult> MaakGebruiker()
        {
            if (!await _roleManager.RoleExistsAsync("Bar"))
            {
                await _roleManager.CreateAsync(new Rol { Name = "Bar" });
            }
            if (!await _roleManager.RoleExistsAsync("Keuken"))
            {
                await _roleManager.CreateAsync(new Rol { Name = "Keuken" });
            }
            if (!await _roleManager.RoleExistsAsync("Zaal"))
            {
                await _roleManager.CreateAsync(new Rol { Name = "Zaal" });
            }
            if (!await _roleManager.RoleExistsAsync("Tafel"))
            {
                await _roleManager.CreateAsync(new Rol { Name = "Tafel" });
            }

            ViewBag.Rollen = new SelectList(await _roleManager.Roles.Where(r => r.Name != "Tafel").ToListAsync(), "Name", "Name");
            return View();
        }

        // 5. Sla de nieuwe gebruiker op in de database (POST)
        [HttpPost]
        public async Task<IActionResult> MaakGebruiker(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var uniekeCode = Guid.NewGuid().ToString("N");
                var nieuweGebruiker = new Gebruiker
                {
                    UserName = uniekeCode, // Tijdelijke gebruikersnaam
                    Naam = model.Naam,
                    UniekeCode = uniekeCode
                };

                // Tijdelijk wachtwoord instellen zodat identity blij is
                var resultaat = await _userManager.CreateAsync(nieuweGebruiker, "Tijdelijk123!");

                if (resultaat.Succeeded)
                {
                    await _userManager.AddToRoleAsync(nieuweGebruiker, model.Rol);
                    
                    // Stuur door naar QR pagina
                    return RedirectToAction("QRPagina", new { code = uniekeCode });
                }

                foreach (var error in resultaat.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Rollen = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(model);
        }

        // 5b. Toon de QR code voor de nieuwe medewerker/tafel
        public async Task<IActionResult> QRPagina(string code)
        {
            if (string.IsNullOrEmpty(code)) return RedirectToAction("Gebruikers");

            // Zoek de user op basis van code en voeg Tafeltoewijzing toe als het een Tafel-rol is
            var user = _context.Users.FirstOrDefault(u => u.UniekeCode == code);
            if (user != null)
            {
                var isInTafelRol = await _userManager.IsInRoleAsync(user, "Tafel");
                if (isInTafelRol)
                {
                    // Kijk welke tafels vrij zijn
                    var beschikbareTafels = await _context.Tafels.ToListAsync();
                    ViewBag.BeschikbareTafels = beschikbareTafels;
                }
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var actionUrl = Url.Action("Activeer", "Account", new { code = code });
            var url = baseUrl + actionUrl;
            ViewBag.ActivatieUrl = url;
            ViewBag.Code = code;
            return View();
        }

        // 5c. Admin koppelt een tafel aan de gebruiker
        [HttpPost]
        public async Task<IActionResult> KoppelTafel(string code, int tafelId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UniekeCode == code);
            var tafel = await _context.Tafels.FindAsync(tafelId);

            if (user != null && tafel != null)
            {
                var toewijzing = new Tafeltoewijzing
                {
                    GebruikerId = user.Id,
                    TafelId = tafelId,
                    TijdstipToegewezen = DateTime.Now
                };
                _context.Tafeltoewijzingen.Add(toewijzing);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("QRPagina", new { code = code });
        }

        // 6. Verwijder een gebruiker (POST)
        [HttpPost]
        public async Task<IActionResult> VerwijderGebruiker(int id)
        {
            var gebruiker = await _userManager.FindByIdAsync(id.ToString());

            if (gebruiker != null)
            {
                if (gebruiker.UserName != User.Identity?.Name)
                {
                    await _userManager.DeleteAsync(gebruiker);
                }
            }

            return RedirectToAction("Gebruikers");
        }

        // 6b. Gebruiker bewerken (GET)
        [HttpGet]
        public async Task<IActionResult> BewerkGebruiker(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var rollen = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Naam = user.Naam,
                UserName = user.UserName,
                Rol = rollen.FirstOrDefault() ?? ""
            };

            ViewBag.Rollen = new SelectList(await _roleManager.Roles.Where(r => r.Name != "Tafel").ToListAsync(), "Name", "Name", model.Rol);
            return View(model);
        }

        // 6c. Gebruiker bewerken (POST)
        [HttpPost]
        public async Task<IActionResult> BewerkGebruiker(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id.ToString());
                if (user == null) return NotFound();

                user.Naam = model.Naam;
                user.UserName = model.UserName;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update rol
                    var huidigeRollen = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, huidigeRollen);
                    await _userManager.AddToRoleAsync(user, model.Rol);

                    return RedirectToAction("Gebruikers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Rollen = new SelectList(await _roleManager.Roles.Where(r => r.Name != "Tafel").ToListAsync(), "Name", "Name", model.Rol);
            return View(model);
        }

        // 7. Tafelbeheer overzicht
        public async Task<IActionResult> Tafels()
        {
            var tafels = await _context.Tafels.OrderBy(t => t.Nummer).ToListAsync();
            return View(tafels);
        }

        // 8. Nieuwe tafel aanmaken (GET)
        [HttpGet]
        public IActionResult NieuweTafel()
        {
            return View();
        }

        // 9. Nieuwe tafel aanmaken (POST)
        [HttpPost]
        public async Task<IActionResult> NieuweTafel(int nummer)
        {
            var nieuweTafel = new Tafel
            {
                Nummer = nummer,
                Actief = false,
                UniekeCode = Guid.NewGuid().ToString("N") // Korte unieke code zonder streepjes
            };
            _context.Tafels.Add(nieuweTafel);
            await _context.SaveChangesAsync();
            return RedirectToAction("Tafels");
        }

        // 10. Tafel activeren/deactiveren toggle
        [HttpPost]
        public async Task<IActionResult> ToggleTafel(int id)
        {
            var tafel = await _context.Tafels.FindAsync(id);
            if (tafel != null)
            {
                tafel.Actief = !tafel.Actief;
                // Genereer een nieuwe code bij elke activering
                if (tafel.Actief && string.IsNullOrEmpty(tafel.UniekeCode))
                {
                    tafel.UniekeCode = Guid.NewGuid().ToString("N");
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Tafels");
        }

        // 11. Activeer alle tafels
        [HttpPost]
        public async Task<IActionResult> ActiveerAlle()
        {
            var tafels = await _context.Tafels.ToListAsync();
            foreach (var tafel in tafels)
            {
                tafel.Actief = true;
                if (string.IsNullOrEmpty(tafel.UniekeCode))
                    tafel.UniekeCode = Guid.NewGuid().ToString("N");
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Tafels");
        }

        // 12. Deactiveer alle tafels
        [HttpPost]
        public async Task<IActionResult> DeactiveerAlle()
        {
            var tafels = await _context.Tafels.ToListAsync();
            foreach (var tafel in tafels)
                tafel.Actief = false;
            await _context.SaveChangesAsync();
            return RedirectToAction("Tafels");
        }

        // 13. QR-codes overzicht (alleen actieve tafels)
        public async Task<IActionResult> QRCodes(int? id)
        {
            var query = _context.Tafels.Where(t => t.Actief);
            
            if (id.HasValue)
            {
                query = query.Where(t => t.Id == id.Value);
            }

            var activeTafels = await query.OrderBy(t => t.Nummer).ToListAsync();
            return View(activeTafels);
        }
    }
}