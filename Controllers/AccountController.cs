using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using test7.Data;
using test7.Models;
using System.Linq;


namespace test7.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext db;

        public AccountController(AppDbContext context)
        {
            db = context;
        }

        public IActionResult SignIn() => View();

        [HttpPost]
        public async Task<IActionResult> SignIn(string username, string password)
        {
            string hash = PasswordHelper.HashPassword(password);
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash);

            if (user != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            // Ajout du statut dans les claimsS
            new Claim(ClaimTypes.Role, user.Statut),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true // Garder la connexion active
            });

                // Redirection en fonction du statut
                if (user.Statut == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (user.Statut == "Livreur")
                {
                    return RedirectToAction("Index", "Livreur");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Identifiants invalides.";
            return View();
        }

        public IActionResult SignUp() => View();

        [HttpPost]
        public IActionResult SignUp(string username, string password, string statut="Client")
        {
            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Nom d'utilisateur déjà pris.";
                return View();
            }

            string hash = PasswordHelper.HashPassword(password);
            db.Users.Add(new User { Username = username, PasswordHash = hash, Statut=statut });
            db.SaveChanges();

            return RedirectToAction("SignIn");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn");
        }
    }
}
