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
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Récupère l'ID de l'utilisateur depuis les claims
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Si votre User utilise un int comme ID
             var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var cart = await _context.Cart
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
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
    }
}