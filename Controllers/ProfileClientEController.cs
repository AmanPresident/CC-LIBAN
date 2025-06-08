using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test7.Data;
using test7.Models;

namespace test7.Controllers
{
    public class ProfileClientEController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProfileClientEController(AppDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ActionResult> Index()
        {
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

            return View(user);
        }

        // POST: ProfileController/Edit - Sauvegarder les modifications du profil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(User user)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("SignIn", "Account");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (existingUser == null)
            {
                return NotFound();
            }

            // Supprimer les erreurs de validation pour les champs non modifiables
            ModelState.Remove("Email");
            ModelState.Remove("Username");
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                try
                {
                    // Conserver l'ancienne image si aucune nouvelle image n'est fournie
                    string oldImagePath = existingUser.ProfileImage;

                    // Mettre à jour les propriétés modifiables
                    existingUser.FullName = user.FullName;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.Address = user.Address;

                    // Gérer la nouvelle image de profil si elle existe
                    if (user.ImageFile != null && user.ImageFile.Length > 0)
                    {
                        // Supprimer l'ancienne image si elle existe
                        if (!string.IsNullOrEmpty(oldImagePath))
                        {
                            var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Sauvegarder la nouvelle image
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "profiles");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + user.ImageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await user.ImageFile.CopyToAsync(fileStream);
                        }

                        existingUser.ProfileImage = "uploads/profiles/" + uniqueFileName;
                    }

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Profil mis à jour avec succès !";
                    return RedirectToAction("Index", "ProfileClient");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Une erreur s'est produite lors de la mise à jour du profil: " + ex.Message);
                }
            }

            return View(user);
        }
    }
}
