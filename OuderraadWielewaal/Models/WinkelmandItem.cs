namespace OuderraadWielewaal.Models
{
    public class WinkelmandItem
    {
        public int ProductId { get; set; }
        public string Naam { get; set; }
        public float Prijs { get; set; }
        public int Aantal { get; set; }

        // Handig extraatje dat automatisch het subtotaal berekent!
        public float Subtotaal => Prijs * Aantal;
    }
}