using System.ComponentModel.DataAnnotations;

namespace OuderraadWielewaal.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Vul een gebruikersnaam in.")]
        public string Gebruikersnaam { get; set; }

        [Required(ErrorMessage = "Vul de volledige naam in.")]
        public string Naam { get; set; }

        [Required(ErrorMessage = "Vul een wachtwoord in.")]
        [DataType(DataType.Password)]
        public string Wachtwoord { get; set; }

        [Required(ErrorMessage = "Kies een rol.")]
        public string Rol { get; set; }
    }
}