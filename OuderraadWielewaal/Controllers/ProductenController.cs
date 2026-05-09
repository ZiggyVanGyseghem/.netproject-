using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OuderraadWielewaal.Data;
using OuderraadWielewaal.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OuderraadWielewaal.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ProductenController : Controller
    {
        private readonly AppDbContext _context;

        public ProductenController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Producten
        public async Task<IActionResult> Index()
        {
            // We halen het actuele (laatste) detail op per product
            var producten = await _context.Producten
                .Include(p => p.Productdetails)
                .ToListAsync();

            return View(producten);
        }

        // GET: Producten/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Producten/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Naam,Prijs,ProductType")] Productdetail detail)
        {
            ModelState.Remove("Product"); // Fix voor validatiefout: Product is null in formulier

            if (ModelState.IsValid)
            {
                var nieuwProduct = new Product();
                
                detail.Tijdstip = DateTime.Now;
                nieuwProduct.Productdetails.Add(detail);

                _context.Producten.Add(nieuwProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(detail);
        }

        // GET: Producten/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Producten
                .Include(p => p.Productdetails)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            var huidigDetail = product.Productdetails.OrderByDescending(pd => pd.Tijdstip).FirstOrDefault();
            
            return View(huidigDetail);
        }

        // POST: Producten/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Naam,Prijs,ProductType")] Productdetail detail)
        {
            if (id != detail.ProductId) return NotFound();

            ModelState.Remove("Product"); // Fix voor validatiefout

            if (ModelState.IsValid)
            {
                // In plaats van het oude detail aan te passen, maken we een NIEUW detail aan met het huidige tijdstip.
                // Dit zorgt ervoor dat oude bestellingen hun oude prijs en naam behouden!
                detail.Tijdstip = DateTime.Now;
                _context.Productdetails.Add(detail);
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(detail);
        }

        // POST: Producten/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Producten
                .Include(p => p.Bestellijnen)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product != null)
            {
                // Als het product al eens besteld is, kunnen we het beter niet zomaar wissen.
                // Voor nu verwijderen we het toch of we laten het afhangen van de implementatie.
                // We verwijderen het gewoon als het nog geen bestellingen heeft.
                if (!product.Bestellijnen.Any())
                {
                    _context.Producten.Remove(product);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Als het wel bestellingen heeft, dan verbergen we het door bijvoorbeeld de prijs op 0 te zetten of naam aan te passen.
                    // Maar voor CRUD houden we het even simpel, of we voegen een "IsActief" toe aan Productdetail.
                    // Aangezien IsActief niet bestaat, gooien we een simpele waarschuwing of we doen niks.
                    TempData["Error"] = "Dit product kan niet worden verwijderd omdat het al in bestellingen is gebruikt.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
