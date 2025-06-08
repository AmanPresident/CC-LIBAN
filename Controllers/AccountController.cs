using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using test7.Data;
using test7.Models;
using test7.Services;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace test7.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext db;
        private readonly IEmailService _emailService;

        public AccountController(AppDbContext context, IEmailService emailService)
        {
            db = context;
            _emailService = emailService;
        }

        public IActionResult SignIn() => View();

        [HttpPost]
        public async Task<IActionResult> SignIn(string username, string password)
        {
            string hash = PasswordHelper.HashPassword(password);
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash);

            if (user != null)
            {
                // Vérifier si l'email est confirmé
                if (!user.IsEmailConfirmed)
                {
                    ViewBag.Error = "Veuillez confirmer votre email  avant de vous connecter. Vérifiez votre boîte de réception.";
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
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
                        IsPersistent = true
                    });

                // Redirection en fonction du statut
                if (user.Statut == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (user.Statut == "Livreur")
                {
                    return RedirectToAction("Index", "Livraison");
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
        public async Task<IActionResult> SignUp(string username, string email, string password, string statut = "Client")
        {
            // Vérifier si le nom d'utilisateur existe déjà
            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Nom d'utilisateur déjà pris.";
                return View();
            }

            // Vérifier si l'email existe déjà
            if (db.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "Cette adresse email est déjà utilisée.";
                return View();
            }

            // Générer un token de confirmation
            string token = GenerateEmailConfirmationToken();
            string hash = PasswordHelper.HashPassword(password);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hash,
                Statut = statut,
                IsEmailConfirmed = false,
                EmailConfirmationToken = token,
                TokenCreatedAt = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            try
            {
                // Envoyer l'email de confirmation
                await _emailService.SendEmailConfirmationAsync(email, username, token);
                ViewBag.Success = "Compte créé avec succès ! Vérifiez votre email pour confirmer votre compte.";
                return View("SignUpConfirmation");
            }
            catch (Exception ex)
            {
                // En cas d'erreur d'envoi d'email, supprimer l'utilisateur créé
                db.Users.Remove(user);
                db.SaveChanges();
                ViewBag.Error = "Erreur lors de l'envoi de l'email de confirmation. Veuillez réessayer.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult ConfirmEmail(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Lien de confirmation invalide.";
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email && u.EmailConfirmationToken == token);

            if (user == null)
            {
                ViewBag.Error = "Token de confirmation invalide.";
                return View();
            }

            // Vérifier si le token n'a pas expiré (24 heures)
            if (user.TokenCreatedAt.HasValue && DateTime.Now.Subtract(user.TokenCreatedAt.Value).TotalHours > 24)
            {
                ViewBag.Error = "Le lien de confirmation a expiré. Veuillez créer un nouveau compte.";
                return View();
            }

            // Confirmer l'email
            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.TokenCreatedAt = null;
            db.SaveChanges();

            ViewBag.Success = "Votre email a été confirmé avec succès ! Vous pouvez maintenant vous connecter.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Adresse email requise.";
                return View("SignUpConfirmation");
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email && !u.IsEmailConfirmed);

            if (user == null)
            {
                ViewBag.Error = "Utilisateur non trouvé ou email déjà confirmé.";
                return View("SignUpConfirmation");
            }

            // Générer un nouveau token
            user.EmailConfirmationToken = GenerateEmailConfirmationToken();
            user.TokenCreatedAt = DateTime.Now;
            db.SaveChanges();

            try
            {
                await _emailService.SendEmailConfirmationAsync(user.Email, user.Username, user.EmailConfirmationToken);
                ViewBag.Success = "Email de confirmation renvoyé avec succès !";
                return View("SignUpConfirmation");
            }
            catch (Exception)
            {
                ViewBag.Error = "Erreur lors de l'envoi de l'email.";
                return View("SignUpConfirmation");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn");
        }

        private string GenerateEmailConfirmationToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] tokenBytes = new byte[32];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }
    }
}