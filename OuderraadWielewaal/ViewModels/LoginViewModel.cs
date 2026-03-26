using System.ComponentModel.DataAnnotations;

namespace OuderraadWielewaal.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vul een gebruikersnaam in.")]
        public string Gebruikersnaam { get; set; }

        [Required(ErrorMessage = "Vul een wachtwoord in.")]
        [DataType(DataType.Password)]
        public string Wachtwoord { get; set; }
    }
}