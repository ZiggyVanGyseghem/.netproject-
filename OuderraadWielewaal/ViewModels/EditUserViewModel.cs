using System.ComponentModel.DataAnnotations;

namespace OuderraadWielewaal.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vul de volledige naam in.")]
        public string Naam { get; set; }

        [Required(ErrorMessage = "De gebruikersnaam is verplicht.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Kies een rol.")]
        public string Rol { get; set; }
    }
}
