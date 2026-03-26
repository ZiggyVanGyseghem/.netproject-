using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OuderraadWielewaal.Models;
using OuderraadWielewaal.ViewModels;
using System.Threading.Tasks;
using System.Linq;

namespace OuderraadWielewaal.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<Gebruiker> _userManager;
        private readonly RoleManager<Rol> _roleManager;

        public AdminController(UserManager<Gebruiker> userManager, RoleManager<Rol> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. Overzicht van alle gebruikers
        public async Task<IActionResult> Index()
        {
            var alleGebruikers = await _userManager.Users.ToListAsync();
            return View(alleGebruikers);
        }

        // 2. Laat het formulier zien om een gebruiker te maken (GET)
        [HttpGet]
        public async Task<IActionResult> MaakGebruiker()
        {
            // Haal alle rollen op voor de dropdown, of maak standaard rollen aan als ze niet bestaan
            if (!await _roleManager.Roles.AnyAsync())
            {
                await _roleManager.CreateAsync(new Rol { Name = "Bar" });
                await _roleManager.CreateAsync(new Rol { Name = "Keuken" });
                await _roleManager.CreateAsync(new Rol { Name = "Zaal" });
            }

            ViewBag.Rollen = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        // 3. Sla de nieuwe gebruiker op in de database (POST)
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

                // Identity regelt het veilig hashen van het wachtwoord!
                var resultaat = await _userManager.CreateAsync(nieuweGebruiker, model.Wachtwoord);

                if (resultaat.Succeeded)
                {
                    // Koppel de gekozen rol aan de gebruiker
                    await _userManager.AddToRoleAsync(nieuweGebruiker, model.Rol);
                    return RedirectToAction("Index");
                }

                foreach (var error in resultaat.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Rollen = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(model);
        }
    }
}