using System.ComponentModel.DataAnnotations;

namespace OuderraadWielewaal.ViewModels
{
    public class ActiveerViewModel
    {
        public string UniekeCode { get; set; }

        [Required(ErrorMessage = "Vul een gewenste gebruikersnaam in.")]
        public string Gebruikersnaam { get; set; }

        [Required(ErrorMessage = "Vul een sterk wachtwoord in.")]
        [DataType(DataType.Password)]
        public string Wachtwoord { get; set; }
    }
}
