using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace test7.Models
{
    public class Produit
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        public string Name { get; set; }

        public string Description { get; set; }
        public string Type { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Prix { get; set; }

        // Relation avec la catégorie
        public int? CategorieId { get; set; }
        public Categorie? Categorie { get; set; }

        public DateTime? DateCreation { get; set; } = DateTime.Now;
        public string? UrlImage { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        // Nouvelles propriétés pour les statistiques
        public int TotalSales { get; set; } = 0; // Total des ventes
        public int ViewCount { get; set; } = 0; // Nombre de vues
        public bool IsTrending { get; set; } = false; // Produit tendance
        public bool IsFeatured { get; set; } = false; // Produit mis en avant

        // Relations pour les statistiques
        public ICollection<ProductLike> Likes { get; set; } = new List<ProductLike>();
        public ICollection<ProductRating> Ratings { get; set; } = new List<ProductRating>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Propriétés calculées
        [NotMapped]
        public int LikeCount => Likes?.Count ?? 0;

        [NotMapped]
        public double AverageRating => Ratings?.Any() == true ? Ratings.Average(r => r.Rating) : 0;

        [NotMapped]
        public int RatingCount => Ratings?.Count ?? 0;

        [NotMapped]
        public int MonthlySales => OrderItems?.Where(oi => oi.Order.OrderDate >= DateTime.Now.AddMonths(-1))
                                            .Sum(oi => oi.Quantity) ?? 0;
    }
}