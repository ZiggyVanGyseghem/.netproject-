using Microsoft.EntityFrameworkCore;
using OuderraadWielewaal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace OuderraadWielewaal.Data
{
    public class AppDbContext : IdentityDbContext<Gebruiker, Rol, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Dit worden jouw tabellen in de database
        public DbSet<Tafel> Tafels { get; set; }
        public DbSet<Tafeltoewijzing> Tafeltoewijzingen { get; set; }
        public DbSet<Product> Producten { get; set; }
        public DbSet<Productdetail> Productdetails { get; set; }
        public DbSet<Bestelling> Bestellingen { get; set; }
        public DbSet<Bestellijn> Bestellijnen { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Hier vertellen we EF Core over de tabellen met meerdere Primary Keys
            // Dit is cruciaal voor Identity!
            base.OnModelCreating(modelBuilder);

            // De Roltoewijzing HasKey mag weg!

            modelBuilder.Entity<Tafeltoewijzing>()
                .HasKey(tt => new { tt.GebruikerId, tt.TafelId, tt.TijdstipToegewezen });

            modelBuilder.Entity<Productdetail>()
                .HasKey(pd => new { pd.ProductId, pd.Tijdstip });

            modelBuilder.Entity<Bestellijn>()
                .HasKey(bl => new { bl.BestellingId, bl.ProductId });

            // --- TESTDATA (SEEDING) ---

            // 1. Drie tafels aanmaken
            modelBuilder.Entity<Tafel>().HasData(
                new Tafel { Id = 1, Nummer = 1 },
                new Tafel { Id = 2, Nummer = 2 },
                new Tafel { Id = 3, Nummer = 3 }
            );

            // 2. Twee basis producten aanmaken (alleen de ID's)
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1 },
                new Product { Id = 2 }
            );

            // 3. De details (naam, prijs, type) van die producten invullen
            modelBuilder.Entity<Productdetail>().HasData(
                new Productdetail
                {
                    ProductId = 1,
                    Tijdstip = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Naam = "Cola",
                    Prijs = 2.50f,
                    ProductType = ProductType.Drank
                },
                new Productdetail
                {
                    ProductId = 2,
                    Tijdstip = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Naam = "Chips Paprika",
                    Prijs = 2.00f,
                    ProductType = ProductType.Versnapering
                }
            );
        }
    }
}