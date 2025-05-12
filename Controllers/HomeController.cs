using System.Diagnostics;
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
