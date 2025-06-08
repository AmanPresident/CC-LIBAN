using System.Security.Claims;
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

        public async Task<IActionResult> Index()
        {
            // Récupérer l'ID de l'utilisateur connecté
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

            return View();
        }
        public async Task<IActionResult> ListeProduit()
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

        // Nouvelle action pour la recherche AJAX
        [HttpGet]
        public async Task<IActionResult> RechercherProduits(string searchTerm)
        {
            var produits = await _context.Produits
                .Include(p => p.Categorie)
                .Where(p => string.IsNullOrEmpty(searchTerm) ||
                           p.Name.Contains(searchTerm) ||
                           p.Description.Contains(searchTerm) ||
                           (p.Categorie != null && p.Categorie.Name.Contains(searchTerm)))
                .ToListAsync();

            return PartialView("_ProduitsTablePartial", produits);
        }

        // Nouvelle action pour afficher les détails d'un produit
        public async Task<IActionResult> DetailsProduit(int id)
        {
            var produit = await _context.Produits
                .Include(p => p.Categorie)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produit == null)
            {
                return NotFound();
            }

            return View(produit);
        }

        public ActionResult AjouterProduit()
        {
            // Récupérer toutes les catégories et les passer à la vue
            var categories = _context.Categorie.ToList();
            ViewBag.Categories = categories;
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
                    Console.WriteLine("image pas bien selectionner ");
                }

                _context.Add(produit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListeProduit));
            }
            ViewBag.Categories = _context.Categorie.ToList();
            return View(produit);
        }

        public ActionResult AjouterCategorie()
        {
            return View();
        }

        // POST: Produit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjouterCategorie(Categorie categorie)
        {
            Console.WriteLine("ModelState is valid? " + ModelState.IsValid);
            if (ModelState.IsValid)
            {
                _context.Add(categorie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AjouterCategorie));
            }
            return View(categorie);
        }

        // GET: Modifier un produit
        public async Task<ActionResult> ModifierProduit(int id)
        {
            var produit = await _context.Produits
                .Include(p => p.Categorie)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produit == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categorie.ToList();
            return View(produit);
        }

        // POST: Modifier un produit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ModifierProduit(int id, Produit produit)
        {
            if (id != produit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduit = await _context.Produits.FindAsync(id);
                    if (existingProduit == null)
                    {
                        return NotFound();
                    }

                    // Conserver l'ancienne image si aucune nouvelle image n'est fournie
                    string oldImagePath = existingProduit.UrlImage;

                    // Mettre à jour les propriétés
                    existingProduit.Name = produit.Name;
                    existingProduit.Description = produit.Description;
                    existingProduit.Type = produit.Type;
                    existingProduit.Prix = produit.Prix;
                    existingProduit.CategorieId = produit.CategorieId;

                    // Gérer la nouvelle image si elle existe
                    if (produit.ImageFile != null)
                    {
                        // Supprimer l'ancienne image si elle existe
                        if (!string.IsNullOrEmpty(oldImagePath))
                        {
                            var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, oldImagePath);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Sauvegarder la nouvelle image
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + produit.ImageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await produit.ImageFile.CopyToAsync(fileStream);
                        }

                        existingProduit.UrlImage = Path.Combine("uploads", "images", uniqueFileName).Replace("\\", "/");
                    }

                    _context.Update(existingProduit);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(DetailsProduit), new { id = produit.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProduitExists(produit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.Categories = _context.Categorie.ToList();
            return View(produit);
        }

        // GET: Supprimer un produit (page de confirmation)
        public async Task<ActionResult> SupprimerProduit(int id)
        {
            var produit = await _context.Produits
                .Include(p => p.Categorie)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produit == null)
            {
                return NotFound();
            }

            return View(produit);
        }

        // POST: Supprimer un produit (confirmation)
        [HttpPost, ActionName("SupprimerProduit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SupprimerProduitConfirm(int id)
        {
            var produit = await _context.Produits.FindAsync(id);
            if (produit != null)
            {
                // Supprimer l'image associée si elle existe
                if (!string.IsNullOrEmpty(produit.UrlImage))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, produit.UrlImage);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Produits.Remove(produit);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ListeProduit));
        }

        // Méthode auxiliaire pour vérifier si un produit existe
        private bool ProduitExists(int id)
        {
            return _context.Produits.Any(e => e.Id == id);
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

        // Ajoutez ces actions à votre DashboardController existant

        // GET: Liste des commandes
        public async Task<IActionResult> ListeCommandes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("SignIn", "Account");
            }

            // Récupérer les informations de l'utilisateur admin
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            // Passer les informations utilisateur à la vue
            ViewBag.UserName = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Username;
            ViewBag.UserRole = user.Statut;
            ViewBag.ProfileImage = !string.IsNullOrEmpty(user.ProfileImage) ? user.ProfileImage : "/Dashboard/img/user.jpg";

            // Récupérer toutes les commandes avec les détails
            var commandes = await _context.Order
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(commandes);
        }

        // GET: Détails d'une commande
        public async Task<IActionResult> DetailsCommande(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("SignIn", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            ViewBag.UserName = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Username;
            ViewBag.UserRole = user.Statut;
            ViewBag.ProfileImage = !string.IsNullOrEmpty(user.ProfileImage) ? user.ProfileImage : "/Dashboard/img/user.jpg";

            var commande = await _context.Order
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (commande == null)
            {
                TempData["ErrorMessage"] = "Commande introuvable";
                return RedirectToAction("ListeCommandes");
            }

            return View(commande);
        }

        // POST: Confirmer le paiement d'une commande
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmerPaiement(int id)
        {
            try
            {
                var commande = await _context.Order
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande introuvable";
                    return RedirectToAction("ListeCommandes");
                }

                // Vérifier que la commande est en attente
                if (commande.Status != OrderStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Cette commande ne peut pas être confirmée dans son état actuel";
                    return RedirectToAction("ListeCommandes");
                }

                // Utiliser une transaction pour assurer la cohérence
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Mettre à jour le statut de la commande
                commande.Status = OrderStatus.Paid;
                commande.PaymentDate = DateTime.Now;

                // Recalculer le montant total pour s'assurer de la cohérence
                decimal montantTotal = 0;
                foreach (var item in commande.OrderItems)
                {
                    // Vérifier le prix actuel du produit (au cas où il aurait changé)
                    var produitActuel = await _context.Produits.FindAsync(item.ProductId);
                    if (produitActuel != null)
                    {
                        // Utiliser le prix au moment de la commande (item.UnitPrice) 
                        // plutôt que le prix actuel pour éviter les discordances
                        item.TotalPrice = item.UnitPrice * item.Quantity;
                        montantTotal += item.TotalPrice;
                    }
                }

                // Ajouter les frais de livraison (vous pouvez ajuster selon votre logique)
                decimal fraisLivraison = 50; // ou récupérer depuis une configuration
                commande.TotalAmount = montantTotal + fraisLivraison;

                _context.Update(commande);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Paiement confirmé pour la commande {commande.OrderNumber}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de la confirmation du paiement: {ex.Message}";
            }

            return RedirectToAction("ListeCommandes");
        }

        // POST: Valider une commande (après paiement)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValiderCommande(int id)
        {
            try
            {
                var commande = await _context.Order.FindAsync(id);
                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande introuvable";
                    return RedirectToAction("ListeCommandes");
                }

                // Vérifier que la commande est payée
                if (commande.Status != OrderStatus.Paid)
                {
                    TempData["ErrorMessage"] = "Cette commande doit être payée avant d'être validée";
                    return RedirectToAction("ListeCommandes");
                }

                commande.Status = OrderStatus.Validated;
                commande.ValidationDate = DateTime.Now;

                _context.Update(commande);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Commande {commande.OrderNumber} validée avec succès";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de la validation: {ex.Message}";
            }

            return RedirectToAction("ListeCommandes");
        }

        // POST: Marquer comme expédiée
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarquerExpediee(int id)
        {
            try
            {
                var commande = await _context.Order.FindAsync(id);
                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande introuvable";
                    return RedirectToAction("ListeCommandes");
                }

                if (commande.Status != OrderStatus.Validated)
                {
                    TempData["ErrorMessage"] = "Cette commande doit être validée avant d'être expédiée";
                    return RedirectToAction("ListeCommandes");
                }

                commande.Status = OrderStatus.Shipped;
                _context.Update(commande);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Commande {commande.OrderNumber} marquée comme expédiée";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de la mise à jour: {ex.Message}";
            }

            return RedirectToAction("ListeCommandes");
        }

        // POST: Annuler une commande
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnnulerCommande(int id, string motif = "")
        {
            try
            {
                var commande = await _context.Order.FindAsync(id);
                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande introuvable";
                    return RedirectToAction("ListeCommandes");
                }

                // On peut annuler une commande seulement si elle n'est pas encore expédiée
                if (commande.Status == OrderStatus.Shipped || commande.Status == OrderStatus.Delivered)
                {
                    TempData["ErrorMessage"] = "Cette commande ne peut plus être annulée";
                    return RedirectToAction("ListeCommandes");
                }

                commande.Status = OrderStatus.Cancelled;
                if (!string.IsNullOrEmpty(motif))
                {
                    commande.Notes += $"\nAnnulée le {DateTime.Now:dd/MM/yyyy HH:mm} - Motif: {motif}";
                }

                _context.Update(commande);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Commande {commande.OrderNumber} annulée";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de l'annulation: {ex.Message}";
            }

            return RedirectToAction("ListeCommandes");
        }

        // GET: Rechercher des commandes
        [HttpGet]
        public async Task<IActionResult> RechercherCommandes(string searchTerm, OrderStatus? status)
        {
            var query = _context.Order
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o =>
                    o.OrderNumber.Contains(searchTerm) ||
                    o.User.FullName.Contains(searchTerm) ||
                    o.User.Username.Contains(searchTerm) ||
                    o.ShippingAddress.Contains(searchTerm));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var commandes = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return PartialView("_CommandesTablePartial", commandes);
        }
    }
}