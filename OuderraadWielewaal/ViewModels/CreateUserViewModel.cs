using System.ComponentModel.DataAnnotations;

namespace OuderraadWielewaal.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Vul de volledige naam in.")]
        public string Naam { get; set; }

        [Required(ErrorMessage = "Kies een rol.")]
        public string Rol { get; set; }
    }
}