using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using test7.Data;
using System.Linq;
using System.Threading.Tasks;
using test7.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace test7.Controllers
{
    [Authorize(Roles = "Client")]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

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
            

            var cart = await _context.Cart
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId));

            if (cart == null)
            {
                cart = new Cart { UserId = int.Parse(userId) };
                _context.Cart.Add(cart);
                await _context.SaveChangesAsync();
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AjouterAuPanier(int productId, int quantity = 1)
        {
            // Récupérer l'ID de l'utilisateur connecté
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Vérifier si le produit existe
            var product = await _context.Produits.FindAsync(productId);
            if (product == null)
            {
                return NotFound("Produit non trouvé");
            }

            // Récupérer le panier de l'utilisateur ou en créer un nouveau
            var cart = await _context.Cart
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Cart.Add(cart);
            }

            // Vérifier si le produit est déjà dans le panier
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                // Si le produit existe déjà, augmenter la quantité
                existingItem.Quantity += quantity;
            }
            else
            {
                // Sinon, ajouter un nouvel item au panier
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    CartId = cart.Id
                });
            }

            // Sauvegarder les changements
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int change)
        {
            var item = await _context.CartItem
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                return Json(new { success = false });
            }

            var newQuantity = item.Quantity + change;

            // Si la quantité devient <= 0, supprimer l'article
            if (newQuantity <= 0)
            {
                _context.CartItem.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    newQuantity = 0,
                    newTotal = 0
                });
            }

            item.Quantity = newQuantity;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                newQuantity = item.Quantity,
                newTotal = item.Product.Prix * item.Quantity
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var item = await _context.CartItem.FindAsync(itemId);
            if (item != null)
            {
                _context.CartItem.Remove(item);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

    }
}