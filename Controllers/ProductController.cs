using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using test7.Data;
using test7.Models;

namespace test7.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Like/Unlike un produit
        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> ToggleLike(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Non autorisé" });

            var existingLike = await _context.ProductLike
                .FirstOrDefaultAsync(pl => pl.ProductId == productId && pl.UserId == userId);

            if (existingLike != null)
            {
                // Unlike
                _context.ProductLike.Remove(existingLike);
            }
            else
            {
                // Like
                var like = new ProductLike
                {
                    ProductId = productId,
                    UserId = userId
                };
                _context.ProductLike.Add(like);
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.ProductLike.CountAsync(pl => pl.ProductId == productId);
            var isLiked = existingLike == null;

            return Json(new { success = true, isLiked, likeCount });
        }

        // POST: Noter un produit
        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateProduct(int productId, int rating, string comment = "")
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Non autorisé" });

            if (rating < 1 || rating > 5)
                return Json(new { success = false, message = "Note invalide" });

            var existingRating = await _context.ProductRating
                .FirstOrDefaultAsync(pr => pr.ProductId == productId && pr.UserId == userId);

            if (existingRating != null)
            {
                // Mettre à jour la note existante
                existingRating.Rating = rating;
                existingRating.Comment = comment;
                existingRating.RatedAt = DateTime.Now;
            }
            else
            {
                // Créer une nouvelle note
                var newRating = new ProductRating
                {
                    ProductId = productId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment
                };
                _context.ProductRating.Add(newRating);
            }

            await _context.SaveChangesAsync();

            // Calculer la nouvelle moyenne
            var averageRating = await _context.ProductRating
                .Where(pr => pr.ProductId == productId)
                .AverageAsync(pr => pr.Rating);

            var ratingCount = await _context.ProductRating
                .CountAsync(pr => pr.ProductId == productId);

            return Json(new
            {
                success = true,
                averageRating = Math.Round(averageRating, 1),
                ratingCount
            });
        }

        // GET: Produits tendance
        public async Task<IActionResult> Trending()
        {
            var trendingProducts = await _context.Produits
                .Include(p => p.Categorie)
                .Include(p => p.Likes)
                .Include(p => p.Ratings)
                .Include(p => p.OrderItems)
                    .ThenInclude(oi => oi.Order)
                .Where(p => p.OrderItems.Any(oi => oi.Order.OrderDate >= DateTime.Now.AddMonths(-1)))
                .OrderByDescending(p => p.OrderItems
                    .Where(oi => oi.Order.OrderDate >= DateTime.Now.AddMonths(-1))
                    .Sum(oi => oi.Quantity))
                .Take(12)
                .ToListAsync();

            return View(trendingProducts);
        }

        // GET: Produits les mieux notés
        public async Task<IActionResult> TopRated()
        {
            var topRatedProducts = await _context.Produits
                .Include(p => p.Categorie)
                .Include(p => p.Ratings)
                .Where(p => p.Ratings.Any())
                .OrderByDescending(p => p.Ratings.Average(r => r.Rating))
                .ThenByDescending(p => p.Ratings.Count)
                .Take(12)
                .ToListAsync();

            return View(topRatedProducts);
        }

        // GET: Produits les plus vendus
        public async Task<IActionResult> BestSellers()
        {
            var bestSellers = await _context.Produits
                .Include(p => p.Categorie)
                .Include(p => p.OrderItems)
                .Where(p => p.OrderItems.Any())
                .OrderByDescending(p => p.OrderItems.Sum(oi => oi.Quantity))
                .Take(12)
                .ToListAsync();

            return View(bestSellers);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}