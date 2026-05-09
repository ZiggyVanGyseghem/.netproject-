using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mollie.Api.Client;
using Mollie.Api.Models.Payment;
using OuderraadWielewaal.Data;
using OuderraadWielewaal.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OuderraadWielewaal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MollieController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MollieController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromForm] string id)
        {
            var mollieClient = new PaymentClient("test_ASHM9vDq92bmmBMaHSdwTvd5Q3Tms7");
            var payment = await mollieClient.GetPaymentAsync(id);

            if (payment == null || string.IsNullOrEmpty(payment.Metadata))
            {
                return Ok();
            }

            int bestellingId;
            if (!int.TryParse(payment.Metadata, out bestellingId))
            {
                return Ok();
            }

            var bestelling = await _context.Bestellingen
                .Include(b => b.Bestellijnen)
                    .ThenInclude(bl => bl.Product)
                        .ThenInclude(p => p.Productdetails)
                .FirstOrDefaultAsync(b => b.Id == bestellingId);

            if (bestelling != null && bestelling.BetaalStatus == BetaalStatus.Open)
            {
                if (payment.Status == PaymentStatus.Paid)
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
                }
                else if (payment.Status == PaymentStatus.Canceled ||
                         payment.Status == PaymentStatus.Expired ||
                         payment.Status == PaymentStatus.Failed)
                {
                    bestelling.BetaalStatus = BetaalStatus.Geannuleerd;
                }

                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
