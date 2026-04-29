using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mollie.Api;
using Mollie.Api.Extensions;
using OuderraadWielewaal.Data;

var builder = WebApplication.CreateBuilder(args);

// Voeg dit blok toe: Verbind met MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30))));
// VOEG DIT BLOK TOE: Koppel ASP.NET Core Identity aan onze AppDbContext
builder.Services.AddIdentity<OuderraadWielewaal.Models.Gebruiker, OuderraadWielewaal.Models.Rol>(options =>
{
    // Tijdelijke makkelijke wachtwoord-eisen (handig tijdens het testen!)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<OuderraadWielewaal.Data.AppDbContext>()
.AddDefaultTokenProviders();
// Add services to the container.
builder.Services.AddControllersWithViews();
// ... de rest van je Program.cs blijft hetzelfde

// Voeg dit toe onder je database configuratie
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Mandje blijft 2 uur bewaard
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Voeg dit toe in Program.cs (bij de andere services)
// Voeg Mollie toe aan de applicatie
builder.Services.AddMollieApi(options => {
    options.ApiKey = builder.Configuration["Mollie:ApiKey"];
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}")
    .WithStaticAssets();
// --- START TESTDATA INJECTEREN (SEEDING) ---
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<OuderraadWielewaal.Models.Gebruiker>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<OuderraadWielewaal.Models.Rol>>();

    // 1. Maak de rol 'Administrator' aan als die nog niet bestaat
    if (!await roleManager.RoleExistsAsync("Administrator"))
    {
        await roleManager.CreateAsync(new OuderraadWielewaal.Models.Rol { Name = "Administrator" });
    }

    // 2. Maak een standaard beheerder aan als die nog niet bestaat
    if (await userManager.FindByNameAsync("admin") == null)
    {
        var beheerder = new OuderraadWielewaal.Models.Gebruiker
        {
            UserName = "admin",
            Naam = "Hoofdbeheerder Wielewaal"
        };

        // Wachtwoord is simpel voor lokaal testen (min. 4 tekens hadden we ingesteld)
        var resultaat = await userManager.CreateAsync(beheerder, "1234");

        if (resultaat.Succeeded)
        {
            await userManager.AddToRoleAsync(beheerder, "Administrator");
        }
    }
}
// --- EINDE TESTDATA INJECTEREN ---

// Dit stond er waarschijnlijk al:
app.Run();

