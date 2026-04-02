using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OuderraadWielewaal.Models
{
    public class Gebruiker : IdentityUser<int>
    {
        // Id, UserName, Email en PasswordHash zitten nu onzichtbaar ingebouwd!

        [Required, MaxLength(255)]
        public string Naam { get; set; }

        [MaxLength(255)]
        public string? UniekeCode { get; set; }

        public DateTime? TijdstipGeactiveerd { get; set; }

        // Relaties
        public ICollection<Tafeltoewijzing> Tafeltoewijzingen { get; set; } = new List<Tafeltoewijzing>();
        public ICollection<Bestelling> Bestellingen { get; set; } = new List<Bestelling>();
    }

    public class Rol : IdentityRole<int>
    {
    }

    public class Tafel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Nummer { get; set; }

        public ICollection<Tafeltoewijzing> Tafeltoewijzingen { get; set; } = new List<Tafeltoewijzing>();
    }

    public class Tafeltoewijzing
    {
        public int GebruikerId { get; set; }
        public Gebruiker Gebruiker { get; set; }

        public int TafelId { get; set; }
        public Tafel Tafel { get; set; }

        public DateTime TijdstipToegewezen { get; set; }
    }

    public class Product
    {
        [Key]
        public int Id { get; set; }

        public ICollection<Productdetail> Productdetails { get; set; } = new List<Productdetail>();
        public ICollection<Bestellijn> Bestellijnen { get; set; } = new List<Bestellijn>();
    }

    public class Productdetail
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime Tijdstip { get; set; }

        [Required, MaxLength(255)]
        public string Naam { get; set; }

        [Required]
        public float Prijs { get; set; }

        [Required]
        public ProductType ProductType { get; set; }
    }

    public class Bestelling
    {
        [Key]
        public int Id { get; set; }

        public int GebruikerId { get; set; }
        public Gebruiker Gebruiker { get; set; }

        public DateTime TijdstipBesteld { get; set; }

        public BestelStatus Status { get; set; }

        public BetaalStatus BetaalStatus { get; set; }

        [MaxLength(255)]
        public string? MolliePaymentId { get; set; }

        public ICollection<Bestellijn> Bestellijnen { get; set; } = new List<Bestellijn>();
    }

    public class Bestellijn
    {
        public int BestellingId { get; set; }
        public Bestelling Bestelling { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int Hoeveelheid { get; set; }
    }

}