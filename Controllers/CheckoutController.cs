using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using test7.Data;
using test7.Models;
using test7.ViewModels;

namespace test7.Controllers
{
    [Authorize(Roles = "Client")]
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _context;

        public CheckoutController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("SignIn", "Account");

            var cart = await GetUserCartAsync(userId);
            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _context.Users.FindAsync(userId);
            var viewModel = new CheckoutViewModel
            {
                ShippingAddress = user?.Address ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                CartItems = cart.Items.Select(item => new CartItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductImage = item.Product.UrlImage,
                    UnitPrice = item.Product.Prix ?? 0,
                    Quantity = item.Quantity,
                    Total = (item.Product.Prix ?? 0) * item.Quantity
                }).ToList()
            };

            viewModel.Subtotal = viewModel.CartItems.Sum(x => x.Total);
            viewModel.Total = viewModel.Subtotal + viewModel.ShippingCost;

            return View(viewModel);
        }

        // POST: Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("SignIn", "Account");

            var cart = await GetUserCartAsync(userId);
            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction("Index", "Cart");
            }

            // CORRECTION 1: Validation manuelle plus stricte
            if (string.IsNullOrWhiteSpace(model.ShippingAddress))
            {
                ModelState.AddModelError("ShippingAddress", "L'adresse de livraison est obligatoire");
            }

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Le numéro de téléphone est obligatoire");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // CORRECTION 2: Utiliser une transaction pour assurer la cohérence
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    // Créer la commande
                    var order = new Order
                    {
                        OrderNumber = GenerateOrderNumber(),
                        UserId = userId,
                        TotalAmount = model.Subtotal + model.ShippingCost,
                        ShippingAddress = model.ShippingAddress,
                        PhoneNumber = model.PhoneNumber,
                        Notes = model.Notes ?? string.Empty,
                        Status = OrderStatus.Pending,
                        OrderDate = DateTime.Now // CORRECTION 3: S'assurer que la date est définie
                    };

                    _context.Order.Add(order);
                    await _context.SaveChangesAsync();

                    // Créer les éléments de commande
                    foreach (var cartItem in cart.Items)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = cartItem.Product.Prix ?? 0,
                            TotalPrice = (cartItem.Product.Prix ?? 0) * cartItem.Quantity
                        };
                        _context.OrderItem.Add(orderItem);
                    }

                    // Vider le panier
                    _context.CartItem.RemoveRange(cart.Items);

                    // CORRECTION 4: Sauvegarder toutes les modifications avant le commit
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // CORRECTION 5: Ajouter un message de succès et logging
                    TempData["SuccessMessage"] = "Commande créée avec succès!";

                    // Debug: Vérifier que l'ordre existe
                    var createdOrder = await _context.Order.FirstOrDefaultAsync(o => o.OrderNumber == order.OrderNumber);
                    if (createdOrder == null)
                    {
                        throw new Exception("Erreur lors de la création de la commande");
                    }

                    // Rediriger vers la confirmation
                    return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
                }
                catch (Exception ex)
                {
                    // CORRECTION 6: Logging plus détaillé
                    ModelState.AddModelError("", $"Erreur lors de la création de la commande: {ex.Message}");

                    // En cas d'erreur, recharger les données du panier
                    model.CartItems = cart.Items.Select(item => new CartItemViewModel
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        ProductImage = item.Product.UrlImage,
                        UnitPrice = item.Product.Prix ?? 0,
                        Quantity = item.Quantity,
                        Total = (item.Product.Prix ?? 0) * item.Quantity
                    }).ToList();

                    model.Subtotal = model.CartItems.Sum(x => x.Total);
                    model.Total = model.Subtotal + model.ShippingCost;

                    return View(model);
                }
            }

            // CORRECTION 7: Recharger les données même si la validation échoue
            model.CartItems = cart.Items.Select(item => new CartItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ProductImage = item.Product.UrlImage,
                UnitPrice = item.Product.Prix ?? 0,
                Quantity = item.Quantity,
                Total = (item.Product.Prix ?? 0) * item.Quantity
            }).ToList();

            model.Subtotal = model.CartItems.Sum(x => x.Total);
            model.Total = model.Subtotal + model.ShippingCost;

            return View(model);
        }

        // GET: Confirmation
        public async Task<IActionResult> Confirmation(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                TempData["ErrorMessage"] = "Numéro de commande manquant";
                return RedirectToAction("Index", "Home");
            }

            var userId = GetCurrentUserId();
            var order = await _context.Order
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == userId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Commande introuvable";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new OrderConfirmationViewModel
            {
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                PhoneNumber = order.PhoneNumber,
                OrderItems = order.OrderItems.Select(oi => new CartItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImage = oi.Product.UrlImage,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    Total = oi.TotalPrice
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: MyOrders - Mes commandes
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetCurrentUserId();
            var orders = await _context.Order
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        private async Task<Cart> GetUserCartAsync(int userId)
        {
            return await _context.Cart
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private static string GenerateOrderNumber()
        {
            return "CMD-" + DateTime.Now.ToString("yyyyMMdd") + "-" +
                   new Random().Next(1000, 9999).ToString();
        }
        [HttpGet]
        public async Task<IActionResult> CancelOrder(string orderNumber)
        {
            var userId = GetCurrentUserId();
            var order = await _context.Order
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == userId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Commande introuvable";
                return RedirectToAction("MyOrders");
            }

            if (order.Status != OrderStatus.Pending)
            {
                TempData["ErrorMessage"] = "Cette commande ne peut plus être annulée";
                return RedirectToAction("MyOrders");
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Commande annulée avec succès";
            return RedirectToAction("MyOrders");
        }
    }
}