using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test7.Data;
using test7.Models;
using test7.Services;

namespace test7.Controllers
{
    public class ProfileClientCController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProfileClientCController(AppDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public ActionResult Index()
        {
            return View();
        }

        // POST: ProfileController/ChangePassword - Sauvegarder le nouveau mot de passe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("SignIn", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            // Vérifier l'ancien mot de passe - utiliser PasswordHash avec votre PasswordHelper
            if (!PasswordHelper.VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "Le mot de passe actuel est incorrect.");
                return View(model);
            }

            // Mettre à jour le mot de passe
            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mot de passe changé avec succès !";
            return RedirectToAction(nameof(Index));
        }

    }
}
