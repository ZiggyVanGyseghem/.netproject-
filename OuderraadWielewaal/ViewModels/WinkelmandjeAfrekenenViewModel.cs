using System.Collections.Generic;

namespace OuderraadWielewaal.ViewModels
{
    // Dit model vangt het hele winkelmandje op
    public class WinkelmandjeAfrekenenViewModel
    {
        // Optioneel: als je het tafelnummer meestuurt vanuit de QR code
        public string TafelNummer { get; set; }

        public List<WinkelmandjeItem> Items { get; set; } = new List<WinkelmandjeItem>();
    }

    // Dit model is 1 regeltje in het winkelmandje (bijv: 3x Cola)
    public class WinkelmandjeItem
    {
        public int ProductId { get; set; }
        public int Hoeveelheid { get; set; }
    }
}