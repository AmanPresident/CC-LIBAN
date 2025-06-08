using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test7.Models
{
    public class ProductLike
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Produit Product { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.Now;

        // Index unique pour éviter les doublons
        // [Index(nameof(ProductId), nameof(UserId), IsUnique = true)]
    }
}