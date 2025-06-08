using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test7.Data;
using test7.Models;

namespace test7.Controllers
{
    [Authorize(Roles = "Client")]
    public class HomeController : Controller
    {
        //ajout
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(AppDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }








        //ajout de await pour retourner les produit dan la base de donnees
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("SignIn", "Account");
            }

            // Récupérer les informations de l'utilisateur
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            

            // Passer les informations utilisateur à la vue
            ViewBag.UserName = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Username;
            ViewBag.UserRole = user.Statut;
            ViewBag.ProfileImage = !string.IsNullOrEmpty(user.ProfileImage) ? user.ProfileImage : "/Dashboard/img/user.jpg";
            var produits = await _context.Produits
                .Include(p => p.Categorie)  // Ceci charge les données de catégorie associées
                .ToListAsync();

            return View(produits);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
