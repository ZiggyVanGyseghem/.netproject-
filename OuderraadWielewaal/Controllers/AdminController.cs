using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OuderraadWielewaal.Models;
using OuderraadWielewaal.ViewModels;
using OuderraadWielewaal.Data;
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
                .Include(b => b.Bestellijnen)
                    .ThenInclude(bl => bl.Product)
                        .ThenInclude(p => p.Productdetails)
                .OrderByDescending(b => b.TijdstipBesteld) // Nieuwste bovenaan
                .ToListAsync();

            return View(alleBestellingen);
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

            ViewBag.Rollen = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        // 5. Sla de nieuwe gebruiker op in de database (POST)
        [HttpPost]
        public async Task<IActionResult> MaakGebruiker(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var nieuweGebruiker = new Gebruiker
                {
                    UserName = model.Gebruikersnaam,
                    Naam = model.Naam
                };

                var resultaat = await _userManager.CreateAsync(nieuweGebruiker, model.Wachtwoord);

                if (resultaat.Succeeded)
                {
                    await _userManager.AddToRoleAsync(nieuweGebruiker, model.Rol);
                    return RedirectToAction("Gebruikers"); // Fix: Stuur terug naar de tabel!
                }

                foreach (var error in resultaat.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Rollen = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(model);
        }

        // 6. Verwijder een gebruiker (POST)
        [HttpPost]
        public async Task<IActionResult> VerwijderGebruiker(string id)
        {
            var gebruiker = await _userManager.FindByIdAsync(id);

            if (gebruiker != null)
            {
                if (gebruiker.UserName != User.Identity?.Name)
                {
                    await _userManager.DeleteAsync(gebruiker);
                }
            }

            return RedirectToAction("Gebruikers"); // Fix: Stuur terug naar de tabel!
        }
    }
}