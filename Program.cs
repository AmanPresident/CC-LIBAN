using Microsoft.AspNetCore.Authentication.Cookies;
using test7.Data;
using test7.Services; // Ajouter cette ligne pour le service d'email

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuration de la politique des cookies
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

// Ajout pour l'authentification
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/SignIn";
        options.AccessDeniedPath = "/Account/SignIn";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
    });

// Après builder.Services.AddAuthentication(...)
builder.Services.AddAuthorization(options =>
{
    // Optionnel : définir une policy nommée
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("ClientOnly", policy =>
        policy.RequireRole("Client"));
});

// Configuration de la base de données
builder.Services.AddDbContext<AppDbContext>();

// NOUVEAU : Enregistrer le service d'email
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy(); // Important pour la gestion des cookies
app.UsePathBase("/");
app.UseRouting();

// L'ordre EST CRUCIAL : Authentication avant Authorization
app.UseAuthentication();
app.UseAuthorization();

// SOLUTION : Supprimer la route personnalisée car elle n'est pas nécessaire
// La route par défaut suffit amplement pour gérer vos actions
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();