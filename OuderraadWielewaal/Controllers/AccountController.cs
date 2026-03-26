using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OuderraadWielewaal.Models;
using OuderraadWielewaal.ViewModels;
using System.Threading.Tasks;

namespace OuderraadWielewaal.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Gebruiker> _signInManager;

        // Dependency Injection: .NET geeft ons automatisch de SignInManager
        public AccountController(SignInManager<Gebruiker> signInManager)
        {
            _signInManager = signInManager;
        }

        // 1. Laat de inlogpagina zien (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 2. Verwerk het formulier als er op 'Inloggen' wordt geklikt (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Identity controleert hier het wachtwoord!
                var result = await _signInManager.PasswordSignInAsync(model.Gebruikersnaam, model.Wachtwoord, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Gelukt? Stuur ze naar de Startpagina
                    return RedirectToAction("Index", "Main");
                }

                // Mislukt? Geef een foutmelding
                ModelState.AddModelError(string.Empty, "Verkeerde gebruikersnaam of wachtwoord.");
            }
            return View(model);
        }

        // 3. Log de gebruiker uit
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Main");
        }
    }
}