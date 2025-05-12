using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test7.Data;
using test7.Models;

namespace test7.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DashboardController(AppDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        // GET: DashboardController

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AjouterProduit()
        {
            return View();
        }


        // POST: Produit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjouterProduit(Produit produit)
        {
            Console.WriteLine("ModelState is valid? " + ModelState.IsValid);
            if (ModelState.IsValid)
            {
                if (produit.ImageFile != null)
                {
                    Console.WriteLine("boucle de image rentrer ");
                    // Créer le dossier s'il n'existe pas
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Générer un nom de fichier unique
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + produit.ImageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Sauvegarder le fichier
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await produit.ImageFile.CopyToAsync(fileStream);
                    }

                    // Stocker le chemin relatif dans la base
                    produit.UrlImage = Path.Combine("uploads", "images", uniqueFileName).Replace("\\", "/");
                }
                else
                {
                    Console.WriteLine("image pas bien selectionner " );
                }

                    _context.Add(produit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AjouterProduit));
            }
            return View(produit);
        }
    

        // GET: DashboardController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DashboardController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DashboardController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DashboardController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
